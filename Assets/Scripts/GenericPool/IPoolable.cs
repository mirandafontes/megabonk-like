namespace GenericPool
{
    public interface IPoolable
    {
        public void OnGetFromPool();

        public void OnReturnToPool();
    }
}
