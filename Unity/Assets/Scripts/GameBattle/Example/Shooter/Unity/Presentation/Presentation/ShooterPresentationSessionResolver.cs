#nullable enable

namespace AbilityKit.Demo.Shooter.View
{
    internal interface IShooterPresentationSessionFactory
    {
        ShooterPresentationSessionContext Create();
    }

    internal sealed class ShooterPresentationSessionFactory : IShooterPresentationSessionFactory
    {
        public ShooterPresentationSessionContext Create()
        {
            return ShooterPresentationSessionContext.CreateDefault();
        }
    }

    public sealed class ShooterPresentationSessionResolver
    {
        private readonly IShooterPresentationSessionFactory _factory;

        public ShooterPresentationSessionResolver()
            : this(null)
        {
        }

        internal ShooterPresentationSessionResolver(IShooterPresentationSessionFactory? factory)
        {
            _factory = factory ?? new ShooterPresentationSessionFactory();
        }

        public ShooterPresentationSessionContext Resolve(ShooterPresentationSessionContext? existing = null)
        {
            var session = existing ?? _factory.Create();
            session.Retain();
            return session;
        }

        public ShooterPresentationSessionContext Resolve(ShooterPresentationFacade presentation)
        {
            return Resolve(ShooterPresentationSessionContext.CreateFromFacade(presentation));
        }

        public void Release(ShooterPresentationSessionContext? session)
        {
            if (session == null)
            {
                return;
            }

            session.Release();
        }
    }
}
