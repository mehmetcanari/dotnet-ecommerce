#!/bin/bash

CONTAINER_NAME="elasticsearch-standalone"
IMAGE_NAME="docker.elastic.co/elasticsearch/elasticsearch:8.13.4"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${BLUE}=== Elasticsearch Container Manager ===${NC}"
}

# Function to check if container exists
container_exists() {
    docker ps -a --format "table {{.Names}}" | grep -q "^${CONTAINER_NAME}$"
}

# Function to check if container is running
container_running() {
    docker ps --format "table {{.Names}}" | grep -q "^${CONTAINER_NAME}$"
}

# Function to wait for Elasticsearch to be ready
wait_for_elasticsearch() {
    print_status "Waiting for Elasticsearch to be ready..."
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if curl -s -f "http://localhost:9200/_cluster/health" > /dev/null 2>&1; then
            print_status "Elasticsearch is ready!"
            return 0
        fi
        
        print_status "Attempt $attempt/$max_attempts - Elasticsearch not ready yet, waiting..."
        sleep 10
        ((attempt++))
    done
    
    print_error "Elasticsearch failed to start within expected time"
    return 1
}

# Start Elasticsearch
start_elasticsearch() {
    print_header
    print_status "Starting Elasticsearch container..."
    
    if container_running; then
        print_warning "Elasticsearch is already running"
        return 0
    fi
    
    if container_exists; then
        print_status "Starting existing container..."
        docker start $CONTAINER_NAME
    else
        print_status "Creating and starting new Elasticsearch container..."
        docker run -d \
            --name $CONTAINER_NAME \
            -p 9200:9200 \
            -p 9300:9300 \
            -e "discovery.type=single-node" \
            -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" \
            -e "xpack.security.enabled=false" \
            -e "xpack.security.transport.ssl.enabled=false" \
            -e "xpack.security.http.ssl.enabled=false" \
            -e "bootstrap.memory_lock=true" \
            -e "TZ=Europe/Istanbul" \
            --ulimit memlock=-1:-1 \
            $IMAGE_NAME
    fi
    
    if [ $? -eq 0 ]; then
        print_status "Container started successfully"
        wait_for_elasticsearch
    else
        print_error "Failed to start Elasticsearch container"
        return 1
    fi
}

# Stop Elasticsearch
stop_elasticsearch() {
    print_header
    print_status "Stopping Elasticsearch container..."
    
    if ! container_exists; then
        print_warning "Elasticsearch container does not exist"
        return 0
    fi
    
    if ! container_running; then
        print_warning "Elasticsearch is not running"
        return 0
    fi
    
    docker stop $CONTAINER_NAME
    if [ $? -eq 0 ]; then
        print_status "Elasticsearch stopped successfully"
    else
        print_error "Failed to stop Elasticsearch"
        return 1
    fi
}

# Restart Elasticsearch
restart_elasticsearch() {
    print_header
    print_status "Restarting Elasticsearch container..."
    
    stop_elasticsearch
    sleep 2
    start_elasticsearch
}

# Show status
show_status() {
    print_header
    
    if ! container_exists; then
        print_warning "Elasticsearch container does not exist"
        return 0
    fi
    
    if container_running; then
        print_status "Elasticsearch is running"
        echo ""
        print_status "Container details:"
        docker ps --filter "name=$CONTAINER_NAME" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
        echo ""
        
        # Check cluster health
        print_status "Cluster health:"
        if curl -s -f "http://localhost:9200/_cluster/health?pretty" > /dev/null 2>&1; then
            curl -s "http://localhost:9200/_cluster/health?pretty"
        else
            print_warning "Cannot connect to Elasticsearch API"
        fi
    else
        print_warning "Elasticsearch is not running"
        echo ""
        print_status "Container exists but is stopped"
        docker ps -a --filter "name=$CONTAINER_NAME" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    fi
}

# Remove container
remove_elasticsearch() {
    print_header
    print_status "Removing Elasticsearch container..."
    
    if ! container_exists; then
        print_warning "Elasticsearch container does not exist"
        return 0
    fi
    
    if container_running; then
        print_status "Stopping container first..."
        docker stop $CONTAINER_NAME
    fi
    
    docker rm $CONTAINER_NAME
    if [ $? -eq 0 ]; then
        print_status "Elasticsearch container removed successfully"
    else
        print_error "Failed to remove Elasticsearch container"
        return 1
    fi
}

# Show logs
show_logs() {
    print_header
    print_status "Showing Elasticsearch logs..."
    
    if ! container_exists; then
        print_error "Elasticsearch container does not exist"
        return 1
    fi
    
    docker logs -f $CONTAINER_NAME
}

# Show help
show_help() {
    print_header
    echo "Usage: $0 {start|stop|restart|status|remove|logs|help}"
    echo ""
    echo "Commands:"
    echo "  start     - Start Elasticsearch container"
    echo "  stop      - Stop Elasticsearch container"
    echo "  restart   - Restart Elasticsearch container"
    echo "  status    - Show container status and cluster health"
    echo "  remove    - Remove Elasticsearch container"
    echo "  logs      - Show container logs"
    echo "  help      - Show this help message"
    echo ""
    echo "Container Name: $CONTAINER_NAME"
    echo "Image: $IMAGE_NAME"
    echo "Ports: 9200 (HTTP), 9300 (Transport)"
}

# Main script logic
case "$1" in
    start)
        start_elasticsearch
        ;;
    stop)
        stop_elasticsearch
        ;;
    restart)
        restart_elasticsearch
        ;;
    status)
        show_status
        ;;
    remove)
        remove_elasticsearch
        ;;
    logs)
        show_logs
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        print_error "Unknown command: $1"
        echo ""
        show_help
        exit 1
        ;;
esac 