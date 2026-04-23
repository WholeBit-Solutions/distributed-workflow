namespace KafkaWorkflow.Consumer.Base.Workflow
{
    public interface IObjectAccessor<T>
        where T : class
    {
        T? Value { get; set; }
    }
}