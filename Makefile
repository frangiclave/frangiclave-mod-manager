.PHONY: build docker_build docker_pull docker_push

build:
	./build.sh

docker_build:
	docker build -t lyrositor/frangiclave-mod-manager .

docker_pull:
	docker pull lyrositor/frangiclave-mod-manager

docker_push: docker_build
	docker push lyrositor/frangiclave-mod-manager
