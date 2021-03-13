Deploy with Terraform
============

Terraform Overview

- Use terraform to deploy to nearly any hosting environment
- Terraform is open source https://github.com/hashicorp/terraform
- For more information go to https://www.terraform.io/

Pass
________
1. Install and initialize **pass**. 

- pass makes managing these deployment parameters extremely easy
- For instructions to install and initialize go to: https://www.passwordstore.org/
- When you export your environment variables via export TF_VAR_variable_name, the variables get pulled in during terraform plan and terraform apply

Terraform for AWS
________
1. Once **pass** is installed and initialized do the following to store the environment variables in pass
::
 pass insert access_key
 pass insert secret_key
 pass insert aws_key_name
 pass insert docker_blazorboilerplate_image
 pass insert docker_sqlserver_image
 pass insert sa_password
 pass insert cert_password
 pass insert ASPNETCORE_ENVIRONMENT
 pass insert Serilog__MinimumLevel__Default

::

2. Set terraform deployment environment variables for use in terraform:
::
 export TF_VAR_access_key=$(pass access_key)
 export TF_VAR_secret_key=$(pass secret_key)
 export TF_VAR_aws_key_name=$(pass aws_key_name)
 export TF_VAR_docker_blazorboilerplate_image=$(pass docker_blazorboilerplate_image)
 export TF_VAR_docker_sqlserver_image=$(pass docker_sqlserver_image)
 export TF_VAR_sa_password=$(pass sa_password)
 export TF_VAR_cert_password=$(pass cert_password)
 export TF_VAR_ASPNETCORE_ENVIRONMENT=$(pass ASPNETCORE_ENVIRONMENT)
 export TF_VAR_Serilog__MinimumLevel__Default=$(pass Serilog__MinimumLevel__Default)

 In Windows use setx instead. e.g. 
 setx TF_VAR_access_key=$(pass access_key)

::


3. Install Terraform at https://learn.hashicorp.com/tutorials/terraform/install-cli?in=terraform/aws-get-started
::
 - follow instructions on page

::

4. Deploy using Terraform
::
 cd ./src/Utils/Terraform/AWS
 terraform init
 terraform plan
 terraform apply

::

Terraform for Azure (Todo)
________
1. Once **pass** is installed and initialized do the following to store the environment variables in pass
::
 todo:
 pass insert param1
 pass insert param2

::

2. Set terraform deployment environment variables for use in terraform:
::
 todo:
 export TF_VAR_param1=$(pass param1)
 export TF_VAR_param2=$(pass param2)

 In Windows use setx instead. e.g. 
 setx TF_VAR_param1=$(pass param1)

::

3. Install Terraform at https://learn.hashicorp.com/tutorials/terraform/install-cli?in=terraform/azure-get-started
::
 - follow instructions on page

::

4. Deploy using Terraform (Todo)
::
 cd ./src/Utils/Terraform/Azure
 terraform init
 terraform plan
 terraform apply

::