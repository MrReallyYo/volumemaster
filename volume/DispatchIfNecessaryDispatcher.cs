using System.Windows.Threading;

namespace VolumeMaster.volume
{
    public class DispatchIfNecessaryDispatcher
    {
        private Dispatcher _dispatcher;
        public DispatchIfNecessaryDispatcher(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Invoke(Action callback)
        {
            if (_dispatcher == Dispatcher.FromThread(Thread.CurrentThread))
            {
                callback.Invoke();
            }
            else
            {
                _dispatcher.Invoke(callback);
            }
        }

        public TResult Invoke<TResult>(Func<TResult> callback)
        {
            if (_dispatcher == Dispatcher.FromThread(Thread.CurrentThread))
            {
                return callback.Invoke();
            }
            else
            {
                return _dispatcher.Invoke(callback);
            }
        }
    }


}

