﻿#region

using Darkages.Types;
using Darkages.Types.Templates;
using Newtonsoft.Json;

#endregion

namespace Darkages.Storage
{
    public class StorageManager
    {
        public static AislingStorage AislingBucket
            = new AislingStorage();

        public static AreaStorage AreaBucket
            = new AreaStorage();

        public static TemplateStorage<ItemTemplate> ItemBucket = new TemplateStorage<ItemTemplate>();

        public static TemplateStorage<MonsterTemplate> MonsterBucket = new TemplateStorage<MonsterTemplate>();

        public static TemplateStorage<MundaneTemplate> MundaneBucket = new TemplateStorage<MundaneTemplate>();

        public static TemplateStorage<NationTemplate> NationBucket = new TemplateStorage<NationTemplate>();

        public static TemplateStorage<PopupTemplate> PopupBucket = new TemplateStorage<PopupTemplate>();

        public static TemplateStorage<Reactor> ReactorBucket = new TemplateStorage<Reactor>();

        public static TemplateStorage<ServerTemplate> ServerArgBucket = new TemplateStorage<ServerTemplate>();

        public static JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
            Formatting = Formatting.Indented
        };

        public static TemplateStorage<SkillTemplate> SkillBucket = new TemplateStorage<SkillTemplate>();

        public static TemplateStorage<SpellTemplate> SpellBucket = new TemplateStorage<SpellTemplate>();

        public static WarpStorage WarpBucket = new WarpStorage();

        public static TemplateStorage<WorldMapTemplate> WorldMapBucket = new TemplateStorage<WorldMapTemplate>();

        static StorageManager()
        {
        }
    }
}