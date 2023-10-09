# Introduction 
This project is for the vNEXT Technical Assessment, which includes the following:
- JdV.vNEXT.Function - Azure Function app project based on the requirements specified
- Deployment Files
  - main.bicep - Main bicep template to deploy all the Azure resources required for the application
  - full-deployment-pipeline.yml - Pipeline definition that will run and deploy all the resources in Azure DevOps
  - addfirewallrule.ps1 - Powershell script to temporarily add agent IP to connect to the Azure SQL Server during deployment
  - removefirewallrule.ps1 - Powershell script to remove the firewall rule created from addfirewallrule.ps1
  - createtable.ps1 - Powershell script that deploys the CreateDeviceTable script
  - CreateDeviceTable.sql - Script to deploy and create the table used by the Function App into the Azure SQL Database