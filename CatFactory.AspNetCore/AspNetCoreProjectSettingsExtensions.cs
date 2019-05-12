using System.Linq;
using CatFactory.ObjectRelationalMapping.Actions;

namespace CatFactory.AspNetCore
{
    public static class AspNetCoreProjectSettingsExtensions
    {
        public static AspNetCoreProjectSettings RemoveAction<TAction>(this AspNetCoreProjectSettings settings) where TAction : IEntityAction
        {
            settings.Actions.Remove(settings.Actions.First(item => item is TAction));

            return settings;
        }
    }
}
