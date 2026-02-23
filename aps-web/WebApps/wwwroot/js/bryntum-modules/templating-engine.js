export default class TemplatingEngine {
    
    /** @typedef {Object} SegmentTemplate
     * 
     *  @property {String} SegmentName
     *  @property {String} Template
     * 
     * */
    
    constructor(/**@type{String}*/a_blockTooltipTemplate,
                /**@type{String}*/a_activityLinkTemplate,
                /**@type{String}*/a_materialLinkTemplate,
                /**@type{String}*/a_capacityTemplate, 
                /**@type{SegmentTemplate[]}*/a_segmentTemplates,
                bryntumSettings) {
        this.m_blockTooltipTemplate = a_blockTooltipTemplate;
        this.m_activityLinkTemplate = a_activityLinkTemplate;
        this.m_materialLinkTemplate = a_materialLinkTemplate;
        this.m_capacityTemplate = a_capacityTemplate;
        this.m_segmentTemplates = a_segmentTemplates;
        this.bryntumSettings = bryntumSettings;
        this.segmentNameToDisplayName = {
            Timing : "Timing",
            Commitment : "Commitment",
            Attributes : "Attribute(s)",
            Status : "Status",
            MaterialStatus : "Material Status",
            Priority : "Priority",
            Buffer : "Buffer",
            PercentFinished : "Percent Finished",
            Process : "Process",
        }
    }
    
    /** @method
     *  
     *  @return {String}
     * */
    generateBlockTooltip(eventRecord) {
        return this.templateObjectUsingFields(this.m_blockTooltipTemplate, eventRecord);
    }
    
    /**@method
     * @return {String}
     * */
    generateSegment(/**@type{String}*/ segmentName, eventRecord)
    {
        let valueByKey = this.getValueByKey(segmentName, eventRecord);
        if (this.m_segmentTemplates.some((templateInfo) => templateInfo.segmentName === segmentName)) {
            if (segmentName === "PercentFinished") {
                let percentageValue = Math.max(Math.min(Math.round(valueByKey), 100), 1);
                return `<div id='progressBarContainer' class='progress-bar'>
                    <span class='progress-bar-text'>${percentageValue}% Finished</span>
                    <div class='progress-bar-fill' style='width: ${percentageValue}%;'></div>
                </div>`
            }
            else if (segmentName === "Process")
            {
                return segmentName;
            }
            else
            {
                let segmentHtml = this.templateObjectUsingFields(this.m_segmentTemplates.find((templateInfo) => templateInfo.segmentName === segmentName).template, eventRecord)
                return this.bryntumSettings.showActivityTitles ? `<strong>${this.segmentNameToDisplayName[segmentName]}</strong>: ${segmentHtml}` : segmentHtml;
            }
        }
        else
        {
            return ""
        }
    }
    
    getValueByKey(key, eventRecord) {
        let detail = eventRecord.dashtPlanning
        let value;
        switch (key)
        {
            case "Timing":  value = detail.activityTiming ?? "Unknown"; break;
            case "Commitment": detail.jobCommitment ?? "Unknown"; break;
            case "Attributes": value = "Unknown"; break;
            case "Process": value = detail.blockDepartment ?? "Unknown"; break;
            case "Status": value = detail.blockProductionStatus ?? "Unknown"; break;
            case "Material Status": detail.oPMaterialStatus ?? "Unknown"; break;
            case "Priority": value = this.getPriorityDescription(detail.priority); break;
            case "Buffer": value = this.getBufferDescription(detail.oPCurrentBufferWarningLevel, detail.oPProjectedBufferWarningLevel, settings); break;
            case "Percent Finished": value = detail.activityPercentFinished.toString() ?? "Unknown"; break;
            default: value = "Unknown"; break;
        }
        return value;
    }

    getPriorityDescription(priority)
    {
        if (priority === null || priority === undefined)
        {
            return "Unknown"
        }
        
        if (priority < 1)
        {
            return "Less than 1"
        }
        else if (priority > 3)
        {
            return "Higher than 3"
        }
        return priority.toString();
    }

    getBufferDescription(currentBufferLevel, projectedBufferLevel)
    {
        if (this.bryntumSettings.penetrationType === 0)
        {
            return currentBufferLevel;
        }
        else
        {
            return projectedBufferLevel;
        }
    }
    
    /** @method 
     * 
     * @return {String}
     * */
    templateObjectUsingFields(/**@type{String}*/ template, object)
    {
        let idx = 0;
        let strs = [];
        let nonTemplateStartIdx = -1
        while (idx < template.length) {
            if (template[idx] === '{')
            {
                if (nonTemplateStartIdx !== -1)
                {
                    strs.push(template.substring(nonTemplateStartIdx, idx));
                    nonTemplateStartIdx = -1
                }
                if (template.length > idx + 2 && template[idx + 1] === '{') {
                    let {Length, Token} = this.templateConsumeToken(template, idx)
                    Token = Token.replace(/\s/g, '')
                    let evald = this.evalIfConditional(Token, object)
                    if (evald !== Token)
                    {
                        if (evald.length === 0)
                        {
                            idx += Length
                            continue;
                        }
                        
                        if (evald[0] === '{' && evald[1] === '{')
                        {
                            Token = evald;
                        }
                        else
                        {
                            let icon = evald.substring(1, evald.length - 1)
                            strs.push(`<i class='fa ${icon}'></i>`)
                            idx += Length;
                            continue;
                        }
                    }
                    if (Length === -1) {
                        return this.concatStrArr(strs)
                    }
                    let nested = this.getNestedObject(object, Token)
                    if (typeof nested == "object" || typeof nested == "function")
                    {
                        nested = ""
                    }
                    strs.push(nested);
                    
                    idx += Length;
                }
                else
                {
                    let {Length, Token} = this.templateConsumeToken(template, idx)
                    if (Length === -1) {
                        this.concatStrArr(strs)
                    }
                    strs.push(`<i class='fa ${Token}'></i>`)
                    idx += Length - 1;
                }
            }
            else
            {
                if (nonTemplateStartIdx === -1)
                {
                    nonTemplateStartIdx = idx
                }
                idx++
            }
        }
        
        if (nonTemplateStartIdx !== -1)
        {
            strs.push(template.substring(nonTemplateStartIdx, idx));
        }
        return this.concatStrArr(strs).replace(/(\r\n|\r|\n)/, "<br/>")
    }
    evalIfConditional(Token, object)
    {
        if (Token[0] === '(')
        {
            let end = Token.indexOf(')');
            if (end !== -1)
            {
                let json = Token.substring(1, end).replaceAll('\'', '"');
                let obj = JSON.parse(json);
                let startAccessor = Token.indexOf('[');
                let endAccessor = Token.indexOf(']');
                if (endAccessor === -1 || startAccessor === -1)
                {
                    return Token;
                }
                let accessor = Token.substring(startAccessor + 1, endAccessor);
                let getValue = obj[this.getNestedObject(object, accessor)];
                if (getValue === undefined || getValue === null)
                {
                    let optionalIdx = Token.indexOf('||')
                    if (optionalIdx !== -1)
                    {
                        let getAfterOptional = Token.substring(optionalIdx + 2)
                        return getAfterOptional.match(/(?<=['"])(.*?)(?=['"])/)[0]
                    }
                    else
                    {
                        return ""
                    }
                }
                else
                {
                    return getValue;
                }
            }
        }
        else
        {
            return Token;
        }
    }
    
    getNestedObject(object, key)
    {
        let split = key.split('.');
        let sub
        for (let i = 0; i < split.length; i++) {
            let s = split[i];
            let lower = `${s[0].toLowerCase()}${s.substring(1, s.length)}`;
            if (sub !== undefined)
            {
                if (sub[lower] !== undefined)
                {
                    sub = sub[lower];
                }
            }
            else
            {
                if (object[lower] !== undefined)
                {
                    sub = object[lower];
                }
            }
        }
        return sub;
    }
    
    /** @method
     * 
     * @return {String}
     * */
    concatStrArr(arr)
    {
        return arr.join("")
    }
    
    /** @method
     *  @return {{Length: Number, Token: String}}
     * */
    templateConsumeToken(/**@type{String}*/ template, /**@type{Number}*/ idx)
    {
        if (template.length > idx + 1)
        {
            let start;
            let end;
            if (template[idx + 1] === '{')
            {
                start = idx + 2;
                //parsing object key
                while (template[idx + 1] !== '}' || template[idx + 2] !== '}')
                {
                    if (template.length <= idx + 2)
                    {
                        //reached end of template without terminating token
                        return {Length: -1, Token: ""};
                    }
                    idx++
                }

                end = idx + 1;
            }
            else
            {
                start = idx + 1;
                while (template[idx + 1] !== '}')
                {
                    if (template.length <= idx + 2)
                    {
                        //reached end of template without terminating token
                        return {Length: -1, Token: ""};
                    }
                    idx++
                }
                end = idx + 1;
                //parsing icon
            }
            return {Length: end-start + 4, Token: template.substring(start, end) };
        }
        else
        {
            //invalid token
            return {Length: -1, Token: ""}
        }
    }
}