using System;

namespace Sample.Options;

public class HangfireSqlServerStorageOptions
{
    public string ConnectionString { get; set; }
    public TimeSpan QueuePollInterval { get; set; }
    public TimeSpan SlidingInvisibilityTimeout { get; set; }
    public TimeSpan JobExpirationCheckInterval { get; set; }
    public TimeSpan CountersAggregateInterval { get; set; }
    public int DashboardJobListLimit { get; set; }
    public TimeSpan TransactionTimeout { get; set; }
    public TimeSpan? CommandTimeout { get; set; }
    public TimeSpan? CommandBatchMaxTimeout { get; set; }
    public TimeSpan InactiveStateExpirationTimeout { get; set; }
    public string? SchemaName { get; set; }
    public bool DisableGlobalLocks { get; set; }
    public bool UseRecommendedIsolationLevel { get; set; }
    public bool EnableHeavyMigrations { get; set; }
    public bool UseFineGrainedLocks { get; set; }
    public bool UseIgnoreDupKeyOption { get; set; }
    public int DeleteExpiredBatchSize { get; set; }
    public bool UseTransactionalAcknowledge { get; set; }
    public bool TryAutoDetectSchemaDependentOptions { get; set; }
    public bool PrepareSchemaIfNecessary { get; set; }
}