variable "event_demo_location" {
  description = "Azure region to deploy resources"
  default     = "East US"
}

variable "event_demo_resource_group_name" {
  description = "Name of the resource group"
  default     = "rg-event-demo"
}

variable "event_demo_cosmos_account_name" {
  description = "Name of the Cosmos DB account"
  default     = "eventdemocosmosacct"
}

variable "event_demo_eventgrid_topic_name" {
  description = "Name of the Event Grid topic"
  default     = "eventdemo-topic"
}

variable "event_demo_orchestrator_app_name" {
  description = "Name of the Orchestrator Azure Function App"
  default     = "func-eventdemo-orchestrator"
}

variable "event_demo_okta_app_name" {
  description = "Name of the Okta Handler Azure Function App"
  default     = "func-eventdemo-okta"
}

variable "event_demo_okta_domain" {
  description = "Okta domain"
  type		  = string
}

variable "event_demo_okta_token" {
  description = "Okta API token"
  type		  = string
  sensitive   = true
}

variable "event_demo_function_key" {
  description = "Provisioning function key stored in Key Vault"
  sensitive   = true
}
