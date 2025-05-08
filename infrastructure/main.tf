terraform {
  backend "azurerm" {
    resource_group_name  = "rg-event-demo"
    storage_account_name = "tfstateacctdemo123"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
  subscription_id = "92672e52-ee20-4d1e-84c1-2c3f0ca94fa1"
}

data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "rg" {
  name     = var.event_demo_resource_group_name
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

resource "azurerm_user_assigned_identity" "function_identity" {
  name                = "uami-eventdemo-func"
  location            = var.event_demo_location
  resource_group_name = azurerm_resource_group.rg.name
}

resource "azurerm_key_vault" "kv" {
  name                        = "kv-eventdemo"
  location                    = var.event_demo_location
  resource_group_name         = azurerm_resource_group.rg.name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false
}

resource "azurerm_key_vault_access_policy" "function_app_access" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_user_assigned_identity.function_identity.principal_id

  secret_permissions = ["Get", "Set", "List"]

  depends_on = [
    azurerm_user_assigned_identity.function_identity,
    azurerm_key_vault.kv
  ]
}

resource "azurerm_key_vault_access_policy" "local_user_access" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = ["Get", "Set", "List"]
}

resource "azurerm_key_vault_secret" "function_key" {
  name         = "FunctionKeyProvision"
  value        = var.event_demo_function_key
  key_vault_id = azurerm_key_vault.kv.id

  depends_on = [
    azurerm_key_vault_access_policy.function_app_access,
    azurerm_key_vault_access_policy.local_user_access
  ]
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
  partition_key_paths = ["/id"]
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

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.function_identity.id]
  }

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    AzureWebJobsStorage      = azurerm_storage_account.storage.primary_connection_string
    CosmosDBConnection       = azurerm_cosmosdb_account.cosmos.primary_sql_connection_string
    EventGridTopicEndpoint   = azurerm_eventgrid_topic.topic.endpoint
    EventGridKey             = azurerm_eventgrid_topic.topic.primary_access_key
    FUNCTION_KEY_PROVISION   = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.function_key.id})"
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

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.function_identity.id]
  }

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