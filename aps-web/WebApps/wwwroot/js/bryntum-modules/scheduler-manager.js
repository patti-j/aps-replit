import {PresetManager, SchedulerPro, ViewPreset, Tooltip, WidgetHelper} from '../build/schedulerpro.wc.module.js';
import SchedulerUtils from './scheduler-utils.js';
import SchedulerZoomManager from './scheduler-zoom-manager.js';
import DataManager from './data-manager.js';
import EventRenderer from './event-renderer.js';
import UIManager from './ui-manager.js';
import DependencyVisibilityManager from './dependency-visibility-manager.js';
import ConfigManager from './config.js';
import EventHandlingManager from './event-handling-manager.js'; // New manager for event handling
import TemplatingEngine from "./templating-engine.js";

class SchedulerManager {
    constructor() {
        /**@type {SchedulerPro & Grid} */
        this.scheduler = null;
        this.configManager = new ConfigManager();
        this.dataManager = new DataManager(this);
        this.eventRenderer = new EventRenderer(this);
        this.uiManager = new UIManager(this);
        this.schedulerZoomManager = new SchedulerZoomManager(this);
        this.dependencyVisibilityManager = new DependencyVisibilityManager(this);
        this.eventHandlingManager = new EventHandlingManager(this); // New manager instance
    }

    async reInitializeScheduler() {
        if (this.configManager.hasValidConfig()) {
            SchedulerUtils.clearElement(this.configManager.elementId);
            await this.initializeSchedulerFromConfig();
        } else {
            throw new Error("Scheduler cannot be reinitialized because initial configuration parameters are missing.");
        }
    }

    async prepareScheduler() {
        this.mask = WidgetHelper.mask(document.body, 'Preparing scheduler data...');
    }

    async initializeScheduler(elementId, bryntumProject, presetDictionary, gridSettings, _dotnetNoteCtxRef, templateInfo) {
        this.closeMaskIfExists();
        this.templatingEngine = new TemplatingEngine(templateInfo.blockTemplate, templateInfo.activityLinkTemplate, templateInfo.materialLinkTemplate, templateInfo.capacityTemplate, templateInfo.segments, bryntumProject.settings)
        window.templatingEngine = this.templatingEngine;
        this.configManager.storeConfig(elementId, bryntumProject, presetDictionary);
        await this.ensureSchedulerInitialized(gridSettings, _dotnetNoteCtxRef);
        return { success: true };
    }
    attachNoteRef(_dotnetNoteCtxRef)
    {
        if (!window.onNoteKeyDown)
        {
            window.onNoteKeyDown = onNoteKeyDown;
        }
        dotnetNoteCtxRef = _dotnetNoteCtxRef;
    }

    async getToolTipHtml(data)
    {
        //although this looks useless its actually required as without it the tooltip html gets cached and will only load the text once
        data.tip = new Tooltip({
            
        });
        
        data.eventRecord.tooltipHTML = window.templatingEngine.generateBlockTooltip(data.eventRecord)
        
        if (data.eventRecord.tooltipHTML.includes("<GanttNotes/>"))
        {
            let note = await dotnetNoteCtxRef.invokeMethodAsync("GetNoteForEvent", data.eventRecord.id)
            let html = data.eventRecord.tooltipHTML.replace("<GanttNotes/>", "");
            html = html + `<div><textarea id='ganttNote${data.eventRecord.id}' rows='6' cols='50' onkeyup='window.onNoteKeyDown(${data.eventRecord.id})'>${note}</textarea></div>`
            return html;
        }
        return data.eventRecord.tooltipHTML;
    }
    
    async ensureSchedulerInitialized(gridSettings, _dotnetNoteCtxRef) {
        const config = this.configManager.getConfig();
        config.presetConfig.features.eventTooltip.template = this.getToolTipHtml;

        if (!this.scheduler) {
            /**@type{ViewPreset[]}*/
            let allViewPresets = PresetManager.getAllDataRecords();
            for (let i = 0; i < allViewPresets.length; i++) {
                if (allViewPresets[i].data.id === "hourAndDay-64by40" || allViewPresets[i].data.id === "hourAndDay-100by40")
                {
                    if (config.hourInterval !== undefined && config.hourInterval !== null) {
                        allViewPresets[i].bottomHeader.increment = config.hourInterval;
                    }
                }
            }
            await this.initializeSchedulerFromConfig(gridSettings);
        }
        else
        {
            let allViewPresets = this.scheduler.presets.allRecords;
            for (let i = 0; i < allViewPresets.length; i++) {
                if (allViewPresets[i].data.id === "hourAndDay-64by40" || allViewPresets[i].data.id === "hourAndDay-100by40")
                {
                    if (config.hourInterval !== undefined && config.hourInterval !== null) {
                        allViewPresets[i].bottomHeader.increment = config.hourInterval;
                        console.log(allViewPresets[i]);
                    }
                }
            }
        }
        this.attachNoteRef(_dotnetNoteCtxRef);

        await this.updateSchedulerDataWithMask(config);
        await this.updateSchedulerDependenciesWithMask(config);
    }

    async initializeSchedulerFromConfig(gridSettings) {
        const config = this.configManager.getConfig();
        this.mask = WidgetHelper.mask(document.body, 'Initializing scheduler...');

        try {
            await this.sleep(100);
            this.scheduler = new SchedulerPro({
                readOnly: true,
                appendTo: config.elementId,
                eventRenderer: this.eventRenderer.render,
                resourceTimeRangeRenderer({ resourceTimeRangeRecord, resourceRecord, renderData }) {
                    return resourceTimeRangeRecord.name;
                },
                ...config.presetConfig
            });
            this.dataManager.initialize();
            this.eventHandlingManager.setupEventListeners();
            this.uiManager.modifyEditor();
            this.uiManager.modifyDependencyTooltip(); 
            this.setBryntumSettings(gridSettings)
        } catch (error) {
            console.error('Error initializing scheduler:', error);
        } finally {
            this.closeMaskIfExists();
        }
    }

    async updateSchedulerDataWithMask(config) {
        this.mask = WidgetHelper.mask(document.body, 'Loading data...');
        try {
            await this.dataManager.updateDataFromConfig(config);
            this.closeMaskIfExists();
            this.mask = WidgetHelper.mask(document.body, 'Loading capacity labels...');
            await this.dataManager.updateResourceTimeRangesFromConfig(config);
        } catch (error) {
            console.error('Error updating scheduler data:', error);
        } finally {
            this.closeMaskIfExists();
        }
    }

    async updateSchedulerDependenciesWithMask(config) {
        this.mask = WidgetHelper.mask(document.body, 'Loading dependencies...');
        try {
            await this.dataManager.updateDependenciesFromConfig(config);
        } catch (error) {
            console.error('Error updating scheduler dependencies:', error);
        } finally {
            this.closeMaskIfExists();
        }
    }
    
    /** @typedef {Object} ColumnVisibility
     * 
     *  @property {String} field
     *  @property {Boolean} hidden
     * */
    
    // \/ technically this could have a custom sort function but i'm not serializing that sorry not sorry \/ 
    /** @typedef {Object} Sorter
     * 
     *  @property {?String} field
     *  @property {?Boolean} ascending
     * */

    /** @typedef {Object} Grouper
     *
     *  @property {?String} field
     *  @property {?Boolean} ascending
     * */

    /** @typedef {Object} DateRange
     * 
     * @property {Number} startMs
     * @property {Number} endMs
     */

    /** @typedef {Object} BryntumGridSettings
     * 
     *  @property {Boolean} hidden
     *  @property {ColumnVisibility[]} columnVisibilities
     *  @property {Sorter[]} sorters
     *  @property {?Grouper} grouper
     *  @property {Number} hourIncrement
     *  @property {?DateRange} dateRange
     *  @property {Number} zoomLevel
     *  @property {String} filterText
     * */
    
    
    /** @returns {BryntumGridSettings}*/
    collectBryntumSettings() {
        /** @type BryntumGridSettings*/
        var result = {
            hidden: false,
            columnVisibilities: this.getColumnVisibility(),
            sorters: [],
            grouper: null,
            dateRange: null,
            filterText: null,
        };
        if (!result.columnVisibilities)
        {
            result.columnVisibilities = []
        }
        
        if (this.scheduler.startDate !== undefined && this.scheduler.startDate !== null) {
            if (this.scheduler.endDate !== undefined && this.scheduler.endDate !== null) {
                result.dateRange = {}
                result.dateRange.startMs = this.scheduler.startDate.valueOf();
                result.dateRange.endMs = this.scheduler.endDate.valueOf();
            }
        }

        if (this.scheduler.zoomLevel !== undefined && this.scheduler.zoomLevel !== null) {
            result.zoomLevel = this.scheduler.zoomLevel;
        }

        if (this.scheduler.eventStore !== undefined && this.scheduler.eventStore !== null) {
            if (this.scheduler.eventStore.filters.first !== undefined && this.scheduler.eventStore.filters.first !== null) {
                result.filterText = this.scheduler.eventStore.filters.first.filterValue
            }
        }
        
        if (this.scheduler.features)
        {
            /** @type FilterBar*/
            let filterBar = this.scheduler.features.filterBar;
            if (filterBar)
            {
                result.hidden = filterBar._hidden;
            }
            
            /** @type Grouper*/
            let grouper = this.scheduler.features.group;
            //NOTE: this is *NOT* the correct way of getting the group information!
            //there should either be groupers on the scheduler store or a single grouper object -
            //on the features containing the field and ascending or a sort function and ascending.
            //however, this is not what is actually happening. Instead, there is a grouper on the -
            //features list which always exists and *never* has the 'field' field set. 
            //I am working around this by checking if groupInfo exists on the groupers store -
            //object and if it does, I read the field from there.
            //See ( https://bryntum.com/products/gantt/docs/api/Grid/feature/Group ) for more info on how this *should* behave
            if (grouper)
            {
                if (grouper.field)
                {
                    result.grouper = {}
                    result.grouper.ascending = grouper.ascending;
                    result.grouper.field = grouper.field;
                }
                else if (grouper.store &&
                    grouper.store.groupInfo &&
                    grouper.store.groupInfo.field &&
                    typeof (grouper.store.groupInfo.field) == "string") 
                {
                    result.grouper = {}
                    result.grouper.ascending = grouper.ascending;
                    result.grouper.field = grouper.store.groupInfo.field;
                }
            }
        }
        
        if (this.scheduler.store)
        {
            if (this.scheduler.store.sorters)
            {
                if (Array.isArray(this.scheduler.store.sorters))
                {
                    /** @type {Sorter[]}*/
                    let sorters = this.scheduler.store.sorters;
                    for (let sorter of sorters) {
                        if ((sorter.ascending != null) && sorter.field &&
                            typeof(sorter.ascending) == "boolean" &&
                            typeof(sorter.field) == "string")
                        {
                            result.sorters.push(
                            {
                                field: sorter.field,
                                ascending: sorter.ascending
                            })
                        }
                    }
                }
                else
                {
                    /** @type {Sorter}*/
                    let sorter = this.scheduler.store.sorters;
                    if ((sorter.ascending != null) && sorter.field &&
                        typeof(sorter.ascending) == "boolean" &&
                        typeof(sorter.field) == "string")
                    {
                        result.sorters.push(
                        {
                            field: sorter.field,
                            ascending: sorter.ascending
                        })
                    }
                }
            }
        }
        
        return result;
    }
    
    /** @param {BryntumGridSettings} config*/
    setBryntumSettings(config) {
        if (!config)
        {
            return;
        }
        
        /** @type FilterBar*/
        let filterBar = this.scheduler.features.filterBar;
        if (filterBar)
        {
            if (config.hidden === true)
            {
                filterBar.hideFilterBar()
            }
            else
            {
                filterBar.showFilterBar()
            }
        }
        
        if (config.dateRange)
        {
            this.scheduler.startDate = new Date(config.dateRange.startMs);
            this.scheduler.endDate = new Date(config.dateRange.endMs);
        }
        
        if (config.zoomLevel)
        {
            this.scheduler.zoomLevel = config.zoomLevel;
        }
        
        if (config.filterText)
        {
            this.scheduler.eventStore.filter({filters: {property: "name", value: config.filterText, operator: "="}, replace: true})
        }
        
        if (config.grouper)
        {
            this.scheduler.store.group({
                field: config.grouper.field,
                ascending: config.grouper.ascending
            })
        }
        
        if (config.sorters.length > 0)
        {
            this.scheduler.store.sort(config.sorters);
        }
    }

    closeMaskIfExists() {
        if (this.mask && this.mask.close) {
            this.mask.close();
        }
    }

    sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    // Method to emit column visibility states
    getColumnVisibility() {
        return Array.from(this.scheduler.columns.storage.values).map(column => ({
            id: column.field,
            hidden: column.hidden
        }));
    }
}
/**
 * @typedef {Object} DotNetObjectReference
 *
 * @property {function(methodName: String, any): Promise<any|None>} invokeMethodAsync
 * */

/** @type{DotNetObjectReference} */
let dotnetNoteCtxRef;
function NoteSaveText(/** @type{String|Number}*/ event, /** @type{String}*/ text)
{
    dotnetNoteCtxRef.invokeMethodAsync("SaveNote", {id: event, text: text});
}

let NoteEditDelayTimeout;
function onNoteKeyDown(/** @type{String|Number}*/ event ) {
    let textArea = document.getElementById("ganttNote"+event);
    let note = textArea.value;
    clearTimeout(NoteEditDelayTimeout);
    NoteEditDelayTimeout = setTimeout( () => {
        NoteSaveText(event, note);
    }, 300)
}
export default SchedulerManager;