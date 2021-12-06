# Before you run this script, uncomment:
# image: localhost:32000/blazorboilerplate:v1.1
# in ../Docker/docker-compose.yml
echo "Did you uncomment 'image: localhost:32000/blazorboilerplate:v1.1' in ../Docker/docker-compose.yml?"
select yn in "Yes" "No"; do
    case $yn in
        Yes ) break;;
        No ) echo "Please do so and restart this script."; exit;;
    esac
done
echo "Did you uncomment ',{'Name': 'Console'}' in appsettings.json? This helps with debugging"
select yn in "Yes" "No"; do
    case $yn in
        Yes ) break;;
        No ) echo "Please do so and restart this script."; exit;;
    esac
done
sudo snap install microk8s --channel=latest/edge --classic
sudo usermod -a -G microk8s $USER
sudo chown -f -R $USER ~/.kube

/usr/bin/newgrp microk8s <<EONG
EONG

microk8s enable registry
microk8s enable dns

cd ../Docker
echo "Do you wish to rebuiid the docker image?"
select yn in "Yes" "No"; do
    case $yn in
        Yes ) docker-compose build; break;;
        No ) break;;
    esac
done
docker push localhost:32000/blazorboilerplate:v1.1
cd ../Microk8s
microk8s kubectl delete deployment.apps/sqlserver
microk8s kubectl delete deployment.apps/blazorboilerplate
microk8s kubectl delete service/blazorboilerplate
microk8s kubectl delete service/sqlserver
echo "Do you wish to delete all the data in the database?"
select yn in "Yes" "No"; do
    case $yn in
        Yes ) microk8s kubectl delete pvc dbdata; break;;
        No ) break;;
    esac
done

echo "Start the deployment?" #causes a delay between delete and deploy
select yn in "Yes" "No"; do
    case $yn in
        Yes ) microk8s.kubectl apply -f blazorboilerplate.yaml; microk8s.kubectl apply -f blazorboilerplate-config.yaml; microk8s.kubectl apply -f sqlserver-config.yaml; break;;
        No ) exit;;
    esac
done

echo "Microk8s Deployment Complete. Naviate to https://non-localhost-ip:30883/"
