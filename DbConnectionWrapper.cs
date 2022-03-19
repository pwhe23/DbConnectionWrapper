namespace DbConnectionWrapper;
using System.Data;

public class ActionDbCommand : WrappedDbCommand
{
    private readonly Action<string?>? _action;

    public ActionDbCommand(IDbCommand cmd, Action<string?>? action = null) : base(cmd)
    {
        _action = action;
    }

    protected override T Execute<T>(Func<T> func)
    {
        _action?.Invoke(CommandText);
        var result = base.Execute(func);
        return result;
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
    public virtual IDbTransaction BeginTransaction(System.Data.IsolationLevel il) => _conn.BeginTransaction(il);
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