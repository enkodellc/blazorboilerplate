namespace BlazorBoilerplate.SourceGenerator
{
    public class EntityGeneratorConfig
    {
        public string EntitiesPath { get; set; }

        public bool GenEntities { get; set; }
        public bool GenInterfaces { get; set; }

        public bool EntityWithInterface { get; set; }
    }
}
