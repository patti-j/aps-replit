using Microsoft.EntityFrameworkCore;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using ReportsWebApp.DB.Services;
using ReportsWebApp.Common;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Internal;
using System.Data;
using System.Reflection;

using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

using ReportsWebApp.Shared;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using ReportsWebApp.DB.Models.WebApp;
using ReportsWebApp.DB.Services.Interfaces;

public class AuditService 
{
    private readonly IAppInsightsLogger _logger;
    private readonly EventHubProducerClient _auditClient;
    private readonly bool enabled;

    //public GroupService(IDbContextFactory<DbReportsContext> dbContext, NotificationService notificationService)
    public AuditService(EventHubProducerClient auditClient, IAppInsightsLogger logger)
    {
        _auditClient = auditClient;
        enabled = auditClient.GetType() == typeof(EventHubProducerClient);
        _logger = logger;
    }

    public record AuditLogRecord(int userId, AuditType type, List<AuditLogPropertyDiff> changedProperties);

    public record AuditLogPropertyDiff(string fieldName, object? original, object? updated);

    public enum AuditType
    {
        New = 0,
        Update = 1,
        Delete = 2
    }

    public async Task SaveAuditRecord(AuditLogRecord record)
    {
        if (!enabled || record == null) return;

        try
        {
            using var eventBatch = await _auditClient.CreateBatchAsync();
            eventBatch.TryAdd(new EventData(JsonConvert.SerializeObject(record)));

            await _auditClient.SendAsync(eventBatch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
        }

    }

    public async Task SaveAuditRecords(List<AuditLogRecord> records)
    {
        if (!enabled || records == null || records.Count == 0) return;
        try
        {
            using var eventBatch = await _auditClient.CreateBatchAsync();

            foreach (AuditLogRecord record in records)
            {
                eventBatch.TryAdd(new EventData(JsonConvert.SerializeObject(record)));
            }

            await _auditClient.SendAsync(eventBatch);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex);
        }
    }

    public AuditLogRecord? CreateEntityUpdateAuditRecord(User initiator, INamedEntity original, INamedEntity updated)
    {
        if (!enabled) return null;

        if (original.GetType() != updated.GetType()) throw new Exception("Cannot parse Audit changes between two different types");

        AuditLogRecord auditRecord;

        if (original.Id == 0) auditRecord = new AuditLogRecord(initiator.Id, AuditType.New, new ());
        else auditRecord = new AuditLogRecord(initiator.Id, AuditType.Update, GetChangedProperties(original, updated));

        return auditRecord;
    }

    private List<PropertyInfo> GetProperties(INamedEntity obj)
    {
        // Only get public instance properties. Ignore calculated properties
        return obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
    }

    private object? GetValueForProperty(INamedEntity obj, PropertyInfo property)
    {
        return property.GetValue(obj);
    }

    private List<AuditLogPropertyDiff> GetChangedProperties(INamedEntity original, INamedEntity updated)
    {
        var diff = new List<AuditLogPropertyDiff>();
        foreach (var prop in GetProperties(original))
        {
            // Skip NotMapped fields
            if (Attribute.IsDefined(prop, typeof(NotMappedAttribute))) continue;

            // Ensure property is a Data field, not a navigation property
            if ((prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string) || prop.PropertyType.IsValueType) && prop.CanWrite)
            {
                var originalVal = GetValueForProperty(original, prop);
                var updatedVal = GetValueForProperty(updated, prop);

                // If the values are not equal, or if original is null but updated isn't, add to the list
                if (!originalVal?.Equals(updatedVal) ?? updatedVal != null)
                {
                    diff.Add(new AuditLogPropertyDiff(prop.Name, originalVal, updatedVal));
                }
            }
        }
        return diff;
    }
}
