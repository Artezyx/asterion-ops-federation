terraform {
  required_version = ">= 1.7.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
}

variable "subscription_id" {
  type      = string
  sensitive = true
}

variable "location" {
  type    = string
  default = "mexicocentral"
}

variable "resource_group_name" {
  type    = string
  default = "rg-cloudsentinel-dev"
}

resource "azurerm_resource_group" "demo" {
  name     = var.resource_group_name
  location = var.location
  tags = {
    project     = "cloudsentinel"
    environment = "development"
  }
}

output "resource_group_name" {
  value = azurerm_resource_group.demo.name
}
