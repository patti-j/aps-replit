using System.Text;

using ReportsWebApp.Common;
using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services
{
    public static class SegmentGenerator
    {
        public static List<Segment> CreateSegmentsFromConfig(SegmentConfig config, DashtPlanning detail, SegmentType segmentType, Dictionary<string, string> segmentColorLookup, BryntumSettings settings, TemplateSegmentContext templateSegmentContext)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (detail == null) throw new ArgumentNullException(nameof(detail));
            if (segmentType == null) return new List<Segment>();

            var segments = new List<Segment>();
            string valueByKey = GetValueByConfigKey(config, detail, settings);
            string templateValued = templateSegmentContext.GetSegmentTemplateValued(segmentType, detail);
            switch (config.Id)
            {
                case SegmentTypeEnum.Attributes:
                    string progressBarHTML = config.Id == SegmentTypeEnum.PercentFinished
                        ? GenerateProgressBarHTML(valueByKey)
                        : string.Empty;
                    segments.AddRange(HandleAttributeSegments(detail, segmentType, templateValued, config, progressBarHTML, settings));
                    break;
                case SegmentTypeEnum.Process:
                    segments.Add(CreateProcessSegment(detail, config, segmentType, segmentColorLookup, settings));
                    break;
                default:
                    segments.Add(CreateSegment(config, settings, valueByKey, segmentColorLookup));
                    break;
            }

            return segments;
        }

        private static string GetValueByConfigKey(SegmentConfig config, DashtPlanning detail, BryntumSettings settings)
        {
            if (config == null || config.IdentifierKey == null || detail == null)
                return "Unknown";

            return config.IdentifierKey switch
            {
                "Timing" => detail.ActivityTiming ?? "Unknown",
                "Commitment" => detail.JobCommitment ?? "Unknown",
                "Attributes" => "Unknown",
                "Process" => detail.BlockDepartment ?? "Unknown",
                "Status" => detail.BlockProductionStatus ?? "Unknown",
                "Material Status" => detail.OPMaterialStatus ?? "Unknown",
                "Priority" => GetPriorityDescription(detail.Priority),
                "Buffer" => GetBufferDescription(detail.OPCurrentBufferWarningLevel, detail.OPProjectedBufferWarningLevel, settings),
                "Percent Finished" => detail.ActivityPercentFinished?.ToString() ?? "Unknown",
                _ => "Unknown"
            };
        }

        private static string GetPriorityDescription(int? priority)
        {
            if (!priority.HasValue)
                return "Unknown";

            return priority switch
            {
                < 1 => "Less than 1",
                > 3 => "Higher than 3",
                _ => priority.ToString()
            };
        }

        private static string GetBufferDescription(string currentBufferLevel, string projectedBufferLevel, BryntumSettings settings)
        {
            return settings.PenetrationType switch
            {
                PenetrationType.CurrentPenetration => currentBufferLevel,
                PenetrationType.ProjectedPenetration => projectedBufferLevel,
                _ => throw new ArgumentException("Invalid Penetration Type")
            };
        }

        private static IEnumerable<Segment> HandleAttributeSegments(DashtPlanning detail, SegmentType segmentType, string templateValued, SegmentConfig config, string progressBarHTML, BryntumSettings settings)
        {
            var attributeConfigs = ExtractAttributeColorCodes(detail, segmentType);

            return attributeConfigs.Select(attrConf =>
                CreateAttributeSegment(detail, config, settings, templateValued, attrConf))
                .Where(segment => segment != null);
        }

        public static Dictionary<string, string> BuildSegmentColorLookup(List<SegmentType> segmentTypes)
        {
            if (segmentTypes == null) throw new ArgumentNullException(nameof(segmentTypes));
            
            return BuildSegment(segmentTypes);
        }

        public static List<Segment> CreateSegmentsFromConfigs(List<SegmentConfig> configs, DashtPlanning detail, Dictionary<string, string> colorLookup, List<SegmentType> segmentTypes, BryntumSettings settings, TemplateSegmentContext templateSegmentContext)
        {
            if (configs == null) throw new ArgumentNullException(nameof(configs));
            if (detail == null) throw new ArgumentNullException(nameof(detail));
            if (colorLookup == null) throw new ArgumentNullException(nameof(colorLookup));

            List <Segment> allSegments = new();
            foreach (var config in configs)
            {
                var segmentType = segmentTypes.FirstOrDefault(s => s.SegmentTypeId == config.Id && s.Show);
                var segments = CreateSegmentsFromConfig(config, detail, segmentType, colorLookup, settings, templateSegmentContext);
                allSegments.AddRange(segments);
            }

            return allSegments;
        }
        
        
        
        public static Segment CreateAttributeSegment(DashtPlanning detail, SegmentConfig config, BryntumSettings settings, string templateValued, AttributeConf attributeConf)
        {
            string formattedColorCode = attributeConf.ColorCode.Trim().StartsWith("#") ? attributeConf.ColorCode.Trim() : $"#{attributeConf.ColorCode.Trim()}";
            string textColor = CommonUtils.GetContrastTextColor(formattedColorCode) ?? config.TextColor;
            string finalValue = attributeConf.ShowAttrSummary ? detail.OPAttributesSummary : templateValued;

            var segment = new Segment
            {
                CustomCode = FormatTitleValue(config, settings, finalValue),
                CustomHTML = true,
                Id = config.Id,
                Color = formattedColorCode,
                TransparentBorder = true,
                SegmentStyle = new CSSType { TextColor = textColor }
            };

            return segment;
        }
        public static Segment CreateSegment(SegmentConfig config, BryntumSettings settings, string valueByKey, Dictionary<string, string> segmentColorLookup)
        {
            string backgroundColor = GetByConfigKey(BuildColorLookupKeyForConfig(config, valueByKey), segmentColorLookup);
            string textColor = CommonUtils.GetContrastTextColor(backgroundColor) ?? config.TextColor;

            return new Segment
            {
                Id = config.Id,
                Color = backgroundColor,
                SegmentStyle = new CSSType { TextColor = textColor }
            };
        }


        public static Segment CreateProcessSegment(DashtPlanning detail, SegmentConfig config, SegmentType segmentType, Dictionary<string, string> segmentColorLookup, BryntumSettings settings)
        {
            var segment = new Segment
            {
                Title = segmentType.Name,
                Id = segmentType.SegmentTypeId,
                Color = "transparent",
                TransparentBorder = true,
                CustomHTML = true,
                SegmentStyle = new CSSType()
            };

            BuildProcessSegmentHTML(detail, segment, config, segmentType, settings, segmentColorLookup);

            return segment;
        }
        
        #region html generation
        public static void BuildProcessSegmentHTML(DashtPlanning detail, Segment segment, SegmentConfig config, SegmentType segmentType, BryntumSettings settings, Dictionary<string, string> segmentColorLookup)
        {
            var sb = new StringBuilder();
            sb.Append("<div style='" + segment.StyleSegmentProcess + "'>");

            var totalHours = CalculateTotalHours(detail, segmentType);

            var validColorMeanings = GetValidColorMeanings(detail, segmentType);

            foreach (var colorMeaning in validColorMeanings)
            {
                AppendColorMeaningHtml(sb, detail, colorMeaning, totalHours, config, settings, segmentColorLookup);
            }

            sb.Append("</div>");
            segment.CustomCode = sb.ToString();
        }

        public static string GenerateProgressBarHTML(string valueByKey)
        {
            return BuildProgressBarHtml(valueByKey);
        }

        public static string BuildTooltipHtml(BryntumSettings settings, TemplateSegmentContext templateSegmentContext)
        {
            var tooltipHtml = new StringBuilder("<div class='tooltip-container'>");

            var evaluatedTemplate = templateSegmentContext.EvaluateTemplate(settings.TooltipDetailsTemplate);
            if(evaluatedTemplate.Success)
                tooltipHtml.AppendLine(evaluatedTemplate.Result);

            tooltipHtml.AppendLine("</div>");
            return tooltipHtml.ToString();
        }

        private static List<ColorMeaning> GetValidColorMeanings(DashtPlanning detail, SegmentType segmentType)
        {
            return segmentType.ColorMeanings
                .Where(colorMeaning => GetStageHours(detail, colorMeaning.Name) > 0)
                .ToList();
        }

        private static void AppendColorMeaningHtml(StringBuilder sb, DashtPlanning detail, ColorMeaning colorMeaning, double totalHours, SegmentConfig config, BryntumSettings settings, Dictionary<string, string> segmentColorLookup)
        {
            double stageHours = GetStageHours(detail, colorMeaning.Name);
            double widthPercentage = CalculateWidthPercentage(stageHours, totalHours);
            string widthStyle = FormatWidthStyle(widthPercentage);
            string color = GetColor(colorMeaning.Name, segmentColorLookup);
            string titleValue = FormatTitleValue(config, settings, $"{stageHours:0.##}h");

            sb.Append($"<div class='p-1 {colorMeaning.Name.ToLower().Replace(" ", "-")}' style='{widthStyle} background-color: {color};'>{titleValue}</div>");
        }

        private static double CalculateWidthPercentage(double stageHours, double totalHours)
        {
            return totalHours > 0 ? stageHours / totalHours * 100 : 0;
        }

        private static string FormatWidthStyle(double widthPercentage)
        {
            return widthPercentage > 0 ? $"width: {widthPercentage:0.##}%;" : "flex-grow: 1;";
        }

        public static string FormatTitleValue(SegmentConfig config, BryntumSettings settings, string templateValued)
        {
            return settings.ShowActivityTitle ? $"<strong>{config.IdentifierKey}</strong>: {templateValued}" : templateValued;
        }

        private static string BuildProgressBarHtml(string valueByKey)
        {
            double percentageValueDouble = double.TryParse(valueByKey, out double result) ? result : 0.0;
            int percentageValue = (int)Math.Round(percentageValueDouble);
            percentageValue = Math.Clamp(percentageValue, 0, 100);

            return $@"
                <div id='progressBarContainer' class='progress-bar'>
                    <span class='progress-bar-text'>{percentageValue}% Finished</span>
                    <div class='progress-bar-fill' style='width: {percentageValue}%;'></div>
                </div>
            ";
        }
        #endregion
        
        #region SegmentExtractor
        public static List<AttributeConf> ExtractAttributeColorCodes(DashtPlanning detail, SegmentType segmentType)
        {
            var attributeColorCodes = new List<AttributeConf>();

            AddColorCodeIfNeeded(attributeColorCodes, segmentType.ShowJobColor, detail.JobColorCode);
            AddColorCodeIfNeeded(attributeColorCodes, segmentType.ShowProductColor, detail.MOProductColorCode);
            AddColorCodeIfNeeded(attributeColorCodes, segmentType.ShowSetupColor, detail.OPSetupColorCode);
            AddCustomAttributeColors(attributeColorCodes, segmentType.ShowCustomAttributeColor, detail.OPAttributesColorCodes);

            return attributeColorCodes;
        }

        private static void AddColorCodeIfNeeded(List<AttributeConf> attributeColorCodes, bool shouldShowColor, string colorCode)
        {
            if (shouldShowColor && !string.IsNullOrWhiteSpace(colorCode))
            {
                attributeColorCodes.Add(new AttributeConf { ColorCode = colorCode, ShowAttrSummary = false });
            }
        }
        private static void AddCustomAttributeColors(List<AttributeConf> attributeColorCodes, bool shouldShowColor, string colorCodes)
        {
            if (shouldShowColor)
            {
                if (!string.IsNullOrWhiteSpace(colorCodes))
                {
                    if (colorCodes.Equals("0"))
                    {
                        // Add transparent color code
                        attributeColorCodes.Add(new AttributeConf { ColorCode = "#00000000", ShowAttrSummary = true });
                    }
                    else 
                    { 
                        var distinctCodes = colorCodes.Split(',')
                            .Select(code => code.Trim())
                            .Distinct();

                        foreach (var code in distinctCodes)
                        {
                            attributeColorCodes.Add(new AttributeConf { ColorCode = code, ShowAttrSummary = true });
                        }
                    }
                }
            }
        }
        public static double ConvertToDecimalHours(string timeValue)
        {
            // Handle non-applicable values
            if (string.IsNullOrWhiteSpace(timeValue) || timeValue.ToLower() == "unknown" || timeValue.ToLower() == "na")
                return 0.0;

            // Attempt to parse the string as a double
            if (double.TryParse(timeValue, out double hours))
                return hours;

            // Log error or throw exception because of unexpected format
            // LogError($"Unexpected time format: {timeValue}");
            return 0.0;
        }
        public static string GetStageValueByMeaning(DashtPlanning detail, string meaning)
        {
            return meaning switch
            {
                "Setup" => detail.BlockSetupHours.ToString(),
                "Run" => detail.BlockRunHours.ToString(),
                "Post-process" => detail.BlockPostProcessingHours.ToString(),
                "Storage" => "N/A",
                "Storage Post-process" => detail.OPStoragePostProcessingHours?.ToString() ?? "N/A",
                _ => "Unknown"
            };
        }

        public static string BuildColorLookupKeyForConfig(SegmentConfig config, string valueByKey)
        {
            return $"{CommonUtils.NormalizeString(config.IdentifierKey)}.{CommonUtils.NormalizeString(valueByKey)}";
        }

        public static double CalculateTotalHours(DashtPlanning detail, SegmentType segmentType)
        {
            return segmentType.ColorMeanings
                .Sum(cm => ConvertToDecimalHours(GetStageValueByMeaning(detail, cm.Name)));
        }

        public static double GetStageHours(DashtPlanning detail, string meaningName)
        {
            return ConvertToDecimalHours(GetStageValueByMeaning(detail, meaningName));
        }
        #endregion
        #region ColorLookup
        
        //this is never added to?
        public static Dictionary<string, string> SegmentColorLookup = new Dictionary<string, string>();
        public static string GetByConfigKey(string value, Dictionary<string, string> segmentColorLookup) =>
            segmentColorLookup.TryGetValue(value, out var color) ? color : "#CCCCCC";


        public static Dictionary<string, string> BuildSegment(IEnumerable<SegmentType> segmentTypes)
        {
            return segmentTypes.SelectMany(st => st.ColorMeanings)
                               .ToDictionary(cm => BuildKey(segmentTypes, cm), cm => cm.Color);
        }

        private static string BuildKey(IEnumerable<SegmentType> segmentTypes, ColorMeaning cm)
        {
            return $"{CommonUtils.NormalizeString(segmentTypes.FirstOrDefault(e => e.SegmentTypeId == cm.SegmentTypeId)?.Name)}.{CommonUtils.NormalizeString(cm.Name)}";
        }
        public static string GetColor(string configKey, Dictionary<string, string> segmentColorLookup)
        {
            string colorCode = GetByConfigKey("Process." + configKey, segmentColorLookup);

            return colorCode;
        }
        #endregion
    }
}
