PROJECT_NAME = MazeDesktop.Desktop
SRC_DIR = $(PROJECT_NAME)
TEST_DIR = ./MazeCore.Tests
DIST_DIR = ../dist
COVERAGE_DIR = coverage

.PHONY: all build install uninstall clean dvi dist tests coverage format check-format


all: build

build:
	dotnet build $(SRC_DIR) -c Release

install: all
	dotnet publish $(SRC_DIR) -c Release -o $(DIST_DIR)

uninstall:
	rm -f $(DIST_DIR)

clean:
	rm -rf $(DIST_DIR)
	rm -rf $(COVERAGE_DIR)
	rm -rf doc
	dotnet clean

dvi:
#	@doxygen -g
	doxygen Doxyfile

dist: clean
	@mkdir -p $(DIST_DIR)
	tar -czf $(DIST_DIR)/$(PROJECT_NAME).tar.gz ./*

tests:
	dotnet test $(TEST_DIR)

coverage:
	dotnet test $(TEST_DIR) /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
	reportgenerator -reports:./**/coverage.opencover.xml -targetdir:$(COVERAGE_DIR)

format:
	cp ../../materials/linters/.clang-format ./
	clang-format -i ./MazeCore/*.cs\
					./AIMazeSolver/*.cs\
					./CommonCore/*.cs\
					./CaveCore/*.cs\
					./MazeCore.Tests/*.cs \
					./MazeDesktop/Controls/*.cs \
					./MazeDesktop/Converters/*.cs \
					./MazeDesktop/ViewModels/*.cs \
					./MazeDesktop/Views/*.cs \
					./MazeDesktop.Desktop/*.cs 
	rm .clang-format

check-format:
	cp ../../materials/linters/.clang-format ./
	clang-format -n ./MazeCore/*.cs\
					./AIMazeSolver/*.cs\
					./CommonCore/*.cs\
					./CaveCore/*.cs\
					./MazeCore.Tests/*.cs \
					./MazeDesktop/Controls/*.cs \
					./MazeDesktop/Converters/*.cs \
					./MazeDesktop/ViewModels/*.cs \
					./MazeDesktop/Views/*.cs \
					./MazeDesktop.Desktop/*.cs 
	rm .clang-format
