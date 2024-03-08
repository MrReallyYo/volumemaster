namespace VolumeMaster.util
{
    class Throttle(long interval = 10)
    {

        private Timer timer = null;
        private long last = 0;


        public void Dispatch(Action action)
        {
            timer?.Dispose();
            timer = null;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (now - last >= interval)
            {
                last = now;
                action();
                return;
            }


            timer = new Timer((obj) =>
            {
                timer?.Dispose();
                last = now;
                action();
            }, null, interval, Timeout.Infinite);
        }
    }
}
