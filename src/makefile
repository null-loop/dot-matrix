build:
	dotnet build --no-restore

restore:
	dotnet restore

pi-run:
	./dotmatrix.pi/bin/Debug/net8.0/dotmatrix.pi

update-go:
	git pull
	make build
	make pi-run