.PHONY: help install dev build start lint lint-fix test test-coverage typecheck clean docker-build docker-up docker-down docker-logs docker-shell setup update-deps

# Colors
GREEN  := $(shell tput -Txterm setaf 2)
YELLOW := $(shell tput -Txterm setaf 3)
RESET  := $(shell tput -Txterm sgr0)

help: ## Show this help
	@echo ''
	@echo 'Usage:'
	@echo '  ${YELLOW}make${RESET} ${GREEN}<target>${RESET}'
	@echo ''
	@echo 'Targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  ${YELLOW}%-15s${RESET} %s\n", $$1, $$2}' $(MAKEFILE_LIST)

install: ## Install dependencies
	npm ci

dev: ## Start development server
	npm run dev

build: ## Build for production
	npm run build

start: ## Start production server
	npm run start

lint: ## Run linting
	npm run lint

lint-fix: ## Fix linting issues
	npm run lint -- --fix

test: ## Run tests
	npm run test

test-coverage: ## Run tests with coverage
	npm run test -- --coverage

typecheck: ## Run TypeScript type checking
	npx tsc --noEmit

clean: ## Clean build artifacts
	rm -rf dist .vite node_modules

# Docker commands
docker-build: ## Build Docker image
	docker build -t fastapi_template_frontend .

docker-up: ## Start all services with Docker Compose
	docker compose -f ../docker-compose.yml up -d

docker-down: ## Stop all Docker Compose services
	docker compose -f ../docker-compose.yml down

docker-logs: ## View Docker Compose logs
	docker compose -f ../docker-compose.yml logs -f

docker-shell: ## Open shell in frontend container
	docker compose -f ../docker-compose.yml exec frontend sh

# Development helpers
setup: install ## Initial project setup
	npx husky install
	@echo "✅ Project setup complete!"

update-deps: ## Update dependencies
	npm update
	npm outdated
