output "function_app_orchestrator_name" {
  value = azurerm_linux_function_app.orchestrator.name
}

output "function_app_okta_handler_name" {
  value = azurerm_linux_function_app.okta_handler.name
}

output "cosmos_db_connection_string" {
  value = azurerm_cosmosdb_account.cosmos.primary_sql_connection_string
  sensitive = true
}

output "event_grid_topic_endpoint" {
  value = azurerm_eventgrid_topic.topic.endpoint
}
