targetScope = 'subscription'

@description('The location of the resources to be deployed')
param location string = 'eastus'

@description('The name of the resource group to be created')
param rgName string = 'rg-ai-magic-demo'

param completionModel string = 'gpt-35-turbo'

param embeddingModel string = 'text-embedding-ada-002'

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
