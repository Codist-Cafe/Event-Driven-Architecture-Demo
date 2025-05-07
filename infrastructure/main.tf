provider "azurerm" {
  features {}
  subscription_id = "92672e52-ee20-4d1e-84c1-2c3f0ca94fa1"
}

resource "azurerm_resource_group" "rg" {
  name     = "${var.event_demo_resource_group_name}"
  location = var.event_demo_location
}

resource "azurerm_storage_account" "storage" {
  name                     = "evtdemostgacct"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = var.event_demo_location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_service_plan" "plan" {
  name                = "event-demo-plan"
  location            = var.event_demo_location
  resource_group_name = azurerm_resource_group.rg.name
  os_type             = "Linux"
  sku_name            = "Y1"
}

resource "azurerm_cosmosdb_account" "cosmos" {
  name                = var.event_demo_cosmos_account_name
  location            = var.event_demo_location
  resource_group_name = azurerm_resource_group.rg.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  consistency_policy {
    consistency_level = "Session"
  }
  geo_location {
    location          = var.event_demo_location
    failover_priority = 0
  }
}

resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "UserDb"
  resource_group_name = azurerm_resource_group.rg.name
  account_name        = azurerm_cosmosdb_account.cosmos.name
}

resource "azurerm_cosmosdb_sql_container" "users" {
  name                = "Users"
  resource_group_name = azurerm_resource_group.rg.name
  account_name        = azurerm_cosmosdb_account.cosmos.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  depends_on          = [azurerm_cosmosdb_account.cosmos]
  partition_key_paths  = ["/id"]
}

resource "azurerm_eventgrid_topic" "topic" {
  name                = var.event_demo_eventgrid_topic_name
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.event_demo_location
}

resource "azurerm_linux_function_app" "orchestrator" {
  name                       = var.event_demo_orchestrator_app_name
  location                   = var.event_demo_location
  resource_group_name        = azurerm_resource_group.rg.name
  service_plan_id            = azurerm_service_plan.plan.id
  storage_account_name       = azurerm_storage_account.storage.name
  storage_account_access_key = azurerm_storage_account.storage.primary_access_key
  
  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    AzureWebJobsStorage      = azurerm_storage_account.storage.primary_connection_string
    CosmosDBConnection = azurerm_cosmosdb_account.cosmos.primary_sql_connection_string
    EventGridTopicEndpoint   = azurerm_eventgrid_topic.topic.endpoint
    EventGridKey             = azurerm_eventgrid_topic.topic.primary_access_key
    FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"
  }
}

resource "azurerm_linux_function_app" "okta_handler" {
  name                       = var.event_demo_okta_app_name
  location                   = var.event_demo_location
  resource_group_name        = azurerm_resource_group.rg.name
  service_plan_id            = azurerm_service_plan.plan.id
  storage_account_name       = azurerm_storage_account.storage.name
  storage_account_access_key = azurerm_storage_account.storage.primary_access_key

  site_config {
    application_stack {
        dotnet_version = "8.0"
    }
  }

  app_settings = {
    AzureWebJobsStorage      = azurerm_storage_account.storage.primary_connection_string
    OktaDomain               = var.event_demo_okta_domain
    OktaToken                = var.event_demo_okta_token
    FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"
  }
}
