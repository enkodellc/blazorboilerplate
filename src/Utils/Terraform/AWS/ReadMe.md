# Terraform
- Use terraform to deploy to aws
- Terraform is open source https://github.com/hashicorp/terraform
- For more information go to https://www.terraform.io/

# Store credentials in environment variables securely (Mac or Linux Optional)
- When you export your environment variables via export TF_VAR_variable_name, the variables get pulled in during terraform plan and terraform apply
- sudo ap-get install pass
- pass init YOUR_GPG_KEY_ID
- pass insert access_key
- pass insert secret_key
- pass insert aws_key_name
- pass insert docker_blazorboilerplate_image
- pass insert docker_sqlserver_image
- pass insert sa_password
- pass insert cert_password
- pass insert ASPNETCORE_ENVIRONMENT
- pass insert Serilog__MinimumLevel__Default
- export TF_VAR_access_key=$(pass access_key)
- export TF_VAR_secret_key=$(pass secret_key)
- export TF_VAR_aws_key_name=$(pass aws_key_name)
- export TF_VAR_docker_blazorboilerplate_image=$(pass docker_blazorboilerplate_image)
- export TF_VAR_docker_sqlserver_image=$(pass docker_sqlserver_image)
- export TF_VAR_sa_password=$(pass sa_password)
- export TF_VAR_cert_password=$(pass cert_password)
- export TF_VAR_ASPNETCORE_ENVIRONMENT=$(pass ASPNETCORE_ENVIRONMENT)
- export TF_VAR_Serilog__MinimumLevel__Default=$(pass Serilog__MinimumLevel__Default)


# Terraform Required
- Install Terraform https://learn.hashicorp.com/tutorials/terraform/install-cli?in=terraform/aws-get-started
- Navigate to ./src/Utils/Terraform/AWS
- terraform init
- terraform plan
- terraform apply
