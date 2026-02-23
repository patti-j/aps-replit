using ReportsWebApp.DB.Models;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ReportsWebApp.DB.Services
{
    public class TemplateSegmentContext
    {
        private DashtPlanning exampleValueDashtPlanning;
        private DashtPlanning exampleValueDashtPlanningTo;
        private DashtResource exampleValueResource;
        private DashtCapacityPlanningShiftsCombined exampleValueCapacity;
        private Dasht_Materials exampleValueMaterial;
        private readonly ConditionalEvaluator m_conditionalEvaluator;
        private BryntumSettings _settings;
        public Dictionary<string, string> exampleData = new Dictionary<string, string>();

        public TemplateSegmentContext(BryntumSettings a_settings)
        {
            _settings = a_settings;
            InitializeExampleValues();
            m_conditionalEvaluator = new ConditionalEvaluator();
        }

        private void InitializeExampleValues()
        {
            exampleValueDashtPlanning = new DashtPlanning();
            exampleValueDashtPlanningTo = new DashtPlanning();
            exampleValueResource = new DashtResource();
            exampleValueCapacity = new DashtCapacityPlanningShiftsCombined();
            exampleValueMaterial = new Dasht_Materials();
        }

        public void SetExamples(
            DashtPlanning exampleValueDashtPlanning = null,
            DashtResource exampleValueResource = null,
            DashtCapacityPlanningShiftsCombined exampleCapacityPlanningShiftsCombined = null,
            Dasht_Materials dasht_Materials = null,
            DashtPlanning dashtPlanningTo = null)
        {
            if (exampleValueDashtPlanning != null)
                this.exampleValueDashtPlanning = exampleValueDashtPlanning;
            if (exampleValueResource != null)
                this.exampleValueResource = exampleValueResource;
            if (exampleCapacityPlanningShiftsCombined != null)
                this.exampleValueCapacity = exampleCapacityPlanningShiftsCombined;
            if (dasht_Materials != null)
                this.exampleValueMaterial = dasht_Materials;
            if (dashtPlanningTo != null)
                this.exampleValueDashtPlanningTo = dashtPlanningTo;

            GenerateData();
            m_conditionalEvaluator.InitializeEngine(exampleData);
        }

        private void GenerateData()
        {
            exampleData = new Dictionary<string, string>();
            AddExampleValuesToDictionary();
            AddPlanningToPropertiesToDictionary();
        }

        private void AddExampleValuesToDictionary()
        {
            foreach (var prop in GetConcatenatedProperties())
            {
                exampleData[GenerateKey(prop)] = GenerateValue(prop);
            }
        }

        private void AddPlanningToPropertiesToDictionary()
        {
            foreach (var prop in exampleValueDashtPlanningTo.GetType().GetProperties())
            {
                exampleData[$"{prop.DeclaringType.Name}To.{prop.Name}"] = GenerateValuePlanningTo(prop);
            }
        }

        public TemplateEvaluation EvaluateTemplate(string templateText)
        {
            var processedTemplateEvaluation = m_conditionalEvaluator.ProcessConditionals(templateText);
            if (!processedTemplateEvaluation.Success)
                return processedTemplateEvaluation;

            processedTemplateEvaluation.Result = ReplaceIconsAndNewlines(processedTemplateEvaluation.Result);
            return processedTemplateEvaluation;
        }

        private List<string> CheckMissingPlaceholders(string template, Dictionary<string, string> data)
        {
            return Regex.Matches(template, @"\[([^\]]+)\]")
                        .Cast<Match>()
                        .Select(m => m.Groups[1].Value)
                        .Where(placeholder => !data.ContainsKey(placeholder))
                        .ToList();
        }

        private string ReplaceIconsAndNewlines(string template)
        {
            template = Regex.Replace(template, @"\{([^\}]+)\}", match => $"<i class='fa {match.Groups[1].Value}'></i>", RegexOptions.Compiled);
            return Regex.Replace(template, @"\r\n|\n|\r", "<br/>", RegexOptions.Compiled);
        }

        private IEnumerable<PropertyInfo> GetConcatenatedProperties()
        {
            return exampleValueDashtPlanning.GetType().GetProperties()
                .Concat(exampleValueResource.GetType().GetProperties())
                .Concat(exampleValueCapacity.GetType().GetProperties())
                .Concat(exampleValueMaterial.GetType().GetProperties());
        }

        private string GenerateKey(PropertyInfo prop)
        {
            return $"{prop.DeclaringType.Name}.{prop.Name}";
        }

        private string GenerateValue(PropertyInfo prop)
        {
            var exampleValue = GetExampleValue(prop);
            return exampleValue?.ToString() ?? "null";
        }
        private string GenerateValuePlanningTo(PropertyInfo prop)
        {
            return prop.GetValue(exampleValueDashtPlanningTo, null)?.ToString() ?? "null";
        }

        private object GetExampleValue(PropertyInfo propertyInfo)
        {
            object instance = propertyInfo.DeclaringType.Name switch
            {
                nameof(DashtPlanning) => exampleValueDashtPlanning,
                nameof(DashtResource) => exampleValueResource,
                nameof(DashtCapacityPlanningShiftsCombined) => exampleValueCapacity,
                nameof(Dasht_Materials) => exampleValueMaterial,
                _ => null
            };
            return propertyInfo.GetValue(instance, null);
        }

        #region TemplateValue
        public void SetSettings(BryntumSettings a_settings)
        {
            _settings = a_settings;
        }
        public string GetDependencyTemplateValued(Dependency dependency, DashtPlanning detail, DashtPlanning to, Dasht_Materials dasht_Materials)
        {
            if (detail == null) throw new ArgumentNullException(nameof(detail));

            string template = dependency.LinkType == LinkType.MaterialsLink ? _settings.MaterialsLinksLabelsTemplate : _settings.ActivityLinksLabelsTemplate;
            SetExamples(detail, null, null, dasht_Materials, to);

            var result = EvaluateTemplate(template);
            if (result.Success)
                return result.Result;
            else
                return result.Message;
        }

        public string GetSegmentTemplateValued(SegmentType segmentType, DashtPlanning detail)
        {
            if (segmentType == null) throw new ArgumentNullException(nameof(segmentType));
            if (detail == null) throw new ArgumentNullException(nameof(detail));

            string template = segmentType.Template;
            SetExamples(detail);

            var result = EvaluateTemplate(template);

            return result.Result;
        }

        internal string GetCapacityTemplateValued(DashtCapacityPlanningShiftsCombined detail, string template)
        {
            if (detail == null) throw new ArgumentNullException(nameof(detail));
            if (string.IsNullOrWhiteSpace(template)) throw new ArgumentNullException(nameof(template));

            SetExamples(exampleCapacityPlanningShiftsCombined: detail);

            var result = EvaluateTemplate(template);

            return result.Result;
        }
        #endregion
    }
}
