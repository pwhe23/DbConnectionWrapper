namespace DbConnectionWrapper;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;

public class ActionDbCommand : WrappedDbCommand
{
    private readonly Action<string?> _action;

    public ActionDbCommand(IDbCommand cmd, Action<string?>? action = null) : base(cmd)
    {
        _action = action ?? (_ => { });
    }

    protected override T Execute<T>(Func<T> func)
    {
        var sb = new StringBuilder();
        SqlParameter? returnValue = null;
        foreach (SqlParameter param in Parameters)
        {
            if (param.Direction == ParameterDirection.ReturnValue)
            {
                returnValue = param;
            }
            sb.AppendLine(FormatParameter(param));
        }
        if (CommandType == CommandType.StoredProcedure && returnValue == null)
        {
            sb.AppendLine($"EXEC {CommandText}");
        }
        else if (CommandType == CommandType.StoredProcedure && returnValue != null)
        {
            sb.AppendLine($"EXEC @{returnValue.ParameterName} = {CommandText}");
        }
        else
        {
            sb.AppendLine(CommandText);
        }
        _action(sb.ToString());

        var result = base.Execute(func);

        sb = new();
        foreach (SqlParameter param in Parameters)
        {
            if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.ReturnValue)
            {
                sb.AppendLine($"SELECT @{param.ParameterName} AS [{param.ParameterName}]");
            }
        }
        _action(sb.ToString());

        return result;
    }

    private static string FormatParameter(SqlParameter param)
    {
        var format = param.SqlDbType switch
        {
            SqlDbType.NVarChar => "DECLARE @{0} {1}({2}) = {3}",
            SqlDbType.VarChar => "DECLARE @{0} {1}({2}) = {3}",
            _ => "DECLARE @{0} {1} = {3}"
        };

        return string.Format(format,
            param.ParameterName,
            param.SqlDbType,
            param.Size == 0 ? 1 : param.Size,
            param.Value is DBNull ? "NULL" : "'" + param.Value + "'"
        );
    }
};

public class WrappedDbConnection : IDbConnection
{
    private readonly IDbConnection _conn;
    private readonly Func<IDbCommand, IDbCommand> _commandCreator = cmd => new WrappedDbCommand(cmd);

    public WrappedDbConnection(IDbConnection conn, Func<IDbCommand, IDbCommand>? commandCreator = null)
    {
        _conn = conn;
        if (commandCreator != null) _commandCreator = commandCreator;
    }

    public virtual string ConnectionString { get => _conn.ConnectionString; set => _conn.ConnectionString = value; }
    public virtual int ConnectionTimeout => _conn.ConnectionTimeout;
    public virtual string Database => _conn.Database;
    public virtual ConnectionState State => _conn.State;
    public virtual IDbTransaction BeginTransaction() => _conn.BeginTransaction();
    public virtual IDbTransaction BeginTransaction(IsolationLevel il) => _conn.BeginTransaction(il);
    public virtual void ChangeDatabase(string databaseName) => _conn.ChangeDatabase(databaseName);
    public virtual void Open() => _conn.Open();
    public virtual void Close() => _conn.Close();
    public virtual void Dispose() => _conn.Dispose();
    public virtual IDbCommand CreateCommand() => _commandCreator(_conn.CreateCommand());
};

public class WrappedDbCommand : IDbCommand
{
    private readonly IDbCommand _cmd;

    public WrappedDbCommand(IDbCommand cmd) => _cmd = cmd;
    public virtual string CommandText { get => _cmd.CommandText; set => _cmd.CommandText = value; }
    public virtual int CommandTimeout { get => _cmd.CommandTimeout; set => _cmd.CommandTimeout = value; }
    public virtual CommandType CommandType { get => _cmd.CommandType; set => _cmd.CommandType = value; }
    public virtual IDbConnection? Connection { get => _cmd.Connection; set => _cmd.Connection = value; }
    public virtual IDbTransaction? Transaction { get => _cmd.Transaction; set => _cmd.Transaction = value; }
    public virtual UpdateRowSource UpdatedRowSource { get => _cmd.UpdatedRowSource; set => _cmd.UpdatedRowSource = value; }
    public virtual void Prepare() => _cmd.Prepare();
    public virtual IDataParameterCollection Parameters => _cmd.Parameters;
    public virtual IDbDataParameter CreateParameter() => _cmd.CreateParameter();
    public virtual void Cancel() => _cmd.Cancel();
    public virtual void Dispose() => _cmd.Dispose();
    public virtual int ExecuteNonQuery() => Execute(() => _cmd.ExecuteNonQuery());
    public virtual IDataReader ExecuteReader() => Execute(() => _cmd.ExecuteReader());
    public virtual IDataReader ExecuteReader(CommandBehavior behavior) => Execute(() => _cmd.ExecuteReader(behavior));
    public virtual object? ExecuteScalar() => _cmd.ExecuteScalar();
    protected virtual T Execute<T>(Func<T> func) => func();
};