namespace KafkaWorkflow.Consumer.Base.Workflow
{
    public class ObjectAccessor<T> : IObjectAccessor<T>
        where T : class
    {
        private static readonly AsyncLocal<ObjectHolder> _currentValue = new();

        public T? Value
        {
            get => _currentValue.Value?.Value;
            set
            {
                if (value != null)
                {
                    _currentValue.Value = new ObjectHolder { Value = value };
                }
                else
                {
                    _currentValue.Value = null; // Explicitly clear
                }
            }
        }

        private sealed class ObjectHolder
        {
            internal T? Value;
        }
    }
}
