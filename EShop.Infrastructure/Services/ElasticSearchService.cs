using Elastic.Clients.Elasticsearch;
using EShop.Application.Abstractions.Services;
using EShop.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EShop.Infrastructure.Services;

internal sealed class ElasticSearchService
    : IElasticSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticSearchService> _logger;
    private readonly ElasticSearchSettings _settings;
    public ElasticSearchService(IOptionsMonitor<ElasticSearchSettings> optionsMonitor, ILogger<ElasticSearchService> logger)
    {
        _settings = optionsMonitor.CurrentValue;
        var settings = new ElasticsearchClientSettings(new Uri(_settings.Host)).DefaultIndex(_settings.DefaultIndex);
        _client = new ElasticsearchClient(settings);
        _logger = logger;
    }

    private async Task CreatIndexIfNotExsist(string indexName)
    {
        var result = await _client.Indices.ExistsAsync(indexName);
        if (!result.Exists)
        {
            await _client.Indices.CreateAsync(indexName);
        }
    }

    public async Task<List<T>> SearchAsync<T>(string searchTerm, string? index = null)
    {
        var result = await _client
            .SearchAsync<T>(s => s
                .Index(index ?? _settings.DefaultIndex)
                .Query(q => q
                    .MultiMatch(m => m
                        .Query(searchTerm)
                        .Fields("*")
                        .Fuzziness(new Fuzziness("AUTO"))
                    )
                )
            );
        return result.Documents.ToList();
    }
    public async Task<bool> AddOrUpdateAsync<T>(T document, string? index = null)
    {
        index = index ?? _settings.DefaultIndex;
        await CreatIndexIfNotExsist(index);
        var result = await _client.IndexAsync(document, indxs =>
        indxs.Index(index).OpType(OpType.Index));
        if (!result.IsValidResponse)
        {
            _logger.LogError("can not create or update document:{document}, index: {index}, Result: {result}",
                document, index, result);
        }
        return result.IsValidResponse;
    }
    public async Task<bool> RemoveAsync(Guid documentId, string? index = null)
    {
        var result = await _client.DeleteAsync(index ?? _settings.DefaultIndex, documentId);
        if (!result.IsValidResponse)
        {
            _logger.LogError("can not remove document with id: {documentId}, index: {index}, Result: {result}",
                documentId, index, result);
        }
        return result.IsValidResponse;
    }
}