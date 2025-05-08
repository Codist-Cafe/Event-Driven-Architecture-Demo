variable "event_demo_location" {
  description = "Azure region to deploy resources"
  default     = "East US"
}

variable "event_demo_resource_group_name" {
  default = "rg-event-demo"
}

variable "event_demo_cosmos_account_name" {
  default = "eventdemocosmosacct"
}

variable "event_demo_eventgrid_topic_name" {
  default = "eventdemo-topic"
}

variable "event_demo_orchestrator_app_name" {
  default = "func-eventdemo-orchestrator"
}

variable "event_demo_okta_app_name" {
  default = "func-eventdemo-okta"
}

variable "event_demo_okta_domain" {
  description = "Okta domain"
  default     = "https://your-org.okta.com"
}

variable "event_demo_okta_token" {
  description = "Okta API token"
  sensitive   = true
}

variable "event_demo_function_key" {
  description = "The provisioning function key stored in Key Vault"
  sensitive   = true
}
