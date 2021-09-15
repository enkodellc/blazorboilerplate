# 1. create vpc
# 2. Create Internet gateway
# 3. Create custom route table
# 4. Create a Subnet
# 5. Associate Subnet with route table
# 6. Create Security Group to allow port 3389,80,443
# 7. Create a network interface with an ip in the subnet that was created in step 4
# 8. Assign an Elastic IP to the network interface created in step 7
# 9. Pull in a docker image and host it
# 10. Monitoring
# 11. Scaling
provider "template" {
  
}
variable "access_key" {
  description = "The access_key for aws"
  type        = string
  sensitive   = true
}
variable "secret_key" {
  description = "The secret_key for aws"
  type        = string
  sensitive   = true
}

variable "aws_key_name" {
  description = "main-key pem file https://us-west-2.console.aws.amazon.com/ec2/v2/home?region=us-west-2#KeyPairs:sort=desc:key-pair-id"
  type        = string
  sensitive   = false
}

variable "docker_blazorboilerplate_image" {
  description = "e.g. dockerusename/blazorboilerplate"
  type        = string
  sensitive   = false
}

variable "docker_sqlserver_image" {
  description = "e.g. mcr.microsoft.com/mssql/server"
  type        = string
  sensitive   = false
}

variable "sa_password" {
  description = "sql server password"
  type        = string
  sensitive   = true
}

variable "cert_password" {
  description = "ssl cert password"
  type        = string
  sensitive   = true
}

variable "ASPNETCORE_ENVIRONMENT" {
  description = "Development OR Production"
  type        = string
  sensitive   = false
}

variable "Serilog__MinimumLevel__Default" {
  description = "Debug OR Production"
  type        = string
  sensitive   = false
}

data "template_file" "docker_compose" {
  template = file("./docker-compose.tpl")
  vars = {
    docker_blazorboilerplate_image = var.docker_blazorboilerplate_image
    docker_sqlserver_image = var.docker_sqlserver_image
    sa_password = var.sa_password
    cert_password = var.cert_password
    ASPNETCORE_ENVIRONMENT = var.ASPNETCORE_ENVIRONMENT
    Serilog__MinimumLevel__Default = var.Serilog__MinimumLevel__Default
  }
}

provider "aws" {
  region = "us-west-2"
  access_key = var.access_key
  secret_key = var.secret_key
}

# 1. create vpc
resource "aws_vpc" "prod-vpc" {
  cidr_block = "10.0.0.0/16"
  tags = {
    "Name" = "Production"
  }
}

# 2. Create Internet gateway
resource "aws_internet_gateway" "gw" {
  vpc_id = aws_vpc.prod-vpc.id
}

# 3. Create custom route table
resource "aws_route_table" "prod-route-table" {
  vpc_id = aws_vpc.prod-vpc.id
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.gw.id
  }
  route {
    ipv6_cidr_block        = "::/0"
    gateway_id = aws_internet_gateway.gw.id
  }
 tags = {
    Name = "prod"
  }
}

# 4. Create a Subnet
resource "aws_subnet" "subnet-1" {
  vpc_id     = aws_vpc.prod-vpc.id
  cidr_block = "10.0.1.0/24"
  availability_zone = "us-west-2a"

  tags = {
    Name = "prod-subnet"
  }
}

# 5. Associate Subnet with route table
resource "aws_route_table_association" "a" {
  subnet_id      = aws_subnet.subnet-1.id
  route_table_id = aws_route_table.prod-route-table.id
}

# 6. Create Security Group to allow port 3389,80,443
resource "aws_security_group" "allow_web" {
  name        = "allow_web_traffic"
  description = "Allow web inbound traffic"
  vpc_id      = aws_vpc.prod-vpc.id

  ingress {
    description = "https"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "http"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  ingress {
    description = "ssh"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "allow_web"
  }
}

# 7. Create a network interface with an ip in the subnet that was created in step 4
resource "aws_network_interface" "web-server-nic" {
  subnet_id       = aws_subnet.subnet-1.id
  private_ips     = ["10.0.1.50"]
  security_groups = [aws_security_group.allow_web.id]
}

# 8. Assign an Elastic IP to the network interface crteated in step 7
resource "aws_eip" "one" {
  vpc                       = true
  network_interface         = aws_network_interface.web-server-nic.id
  associate_with_private_ip = "10.0.1.50"
  depends_on = [aws_internet_gateway.gw]
}

#9 Payload

resource "aws_instance" "web-server-instance" {
  ami           = "ami-0ca5c3bd5a268e7db"
  instance_type = "t2.small"
  availability_zone = "us-west-2a"
  key_name = var.aws_key_name

  network_interface {
    device_index = 0
    network_interface_id = aws_network_interface.web-server-nic.id

  }
  user_data = templatefile("bash.tmpl", { docker_compose = data.template_file.docker_compose.rendered } )


 tags = {
     name = "web-server"
 }
}
