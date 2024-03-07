targetScope = 'subscription'

@description('The location of the resources to be deployed')
param location string = 'eastus'

@description('The name of the resource group to be created')
param rgName string = 'rg-ai-magic-demo'

param completionModel string = 'gpt-35-turbo'

param embeddingModel string = 'text-embedding-ada-002'

param embeddingApiVersion string = '2023-05-15'

resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: rgName
  location: location
}

var suffix = uniqueString(rg.id)

module storage 'modules/storage/storage.bicep' = {
  scope: resourceGroup(rg.name)
  name: 'storage'
  params: {
    location: location
    suffix: suffix
  }
}

module search 'modules/cognitive/aisearch.bicep' = {
  scope: resourceGroup(rg.name)
  name: 'search'
  params: {
    location: location
    suffix: suffix
  }
}

module monitoring 'modules/monitoring/insights.bicep' = {
  scope: resourceGroup(rg.name)
  name: 'monitoring'
  params: {
    location: location
    suffix: suffix
  }
}

module openAi 'modules/cognitive/openai.bicep' = {
  scope: resourceGroup(rg.name)
  name: 'openai'
  params: {
    completionModel: completionModel
    embeddingModel: embeddingModel
    location: location
    suffix: suffix
  }
}

module function 'modules/function/function.bicep' = {
  scope: resourceGroup(rg.name)
  name: 'function'
  params: {
    aiServiceName: openAi.outputs.openAIName
    appInsightname: monitoring.outputs.appInsightsName
    embeddingApiVersion: embeddingApiVersion
    embeddingModel: embeddingModel
    location: location
    storageName: storage.outputs.storageName
    suffix: suffix
  }
}


output functionName string = function.outputs.functionName
