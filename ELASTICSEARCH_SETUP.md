# Elasticsearch Standalone Container Setup

In this project, Elasticsearch runs as a standalone Docker container, separate from Docker Compose.

## 🖥️ System Requirements

### Supported Operating Systems
- ✅ **macOS** (10.14+)
- ✅ **Linux** (Ubuntu, CentOS, Debian, etc.)
- ✅ **WSL** (Windows Subsystem for Linux)
- ✅ **Git Bash** (Windows)

### Not Supported
- ❌ **Windows Command Prompt** (cmd)
- ❌ **Windows PowerShell** (native)

### Prerequisites
- Docker Desktop or Docker Engine installed
- Bash shell available
- curl command available (for health checks)

## 🚀 Quick Start

### Starting Elasticsearch
```bash
./elasticsearch-manager.sh start
```

### Checking Status
```bash
./elasticsearch-manager.sh status
```

### Stopping Elasticsearch
```bash
./elasticsearch-manager.sh stop
```

## 📋 Management Commands

You can use the `elasticsearch-manager.sh` script to manage the Elasticsearch container:

| Command | Description |
|---------|-------------|
| `start` | Starts the Elasticsearch container |
| `stop` | Stops the Elasticsearch container |
| `restart` | Restarts the Elasticsearch container |
| `status` | Shows container status and cluster health |
| `logs` | Shows container logs |
| `remove` | Completely removes the container |
| `help` | Shows help message |

## 🔧 Configuration

### Container Information
- **Container Name**: `elasticsearch-standalone`
- **Image**: `docker.elastic.co/elasticsearch/elasticsearch:8.13.4`
- **HTTP Port**: `9200`
- **Transport Port**: `9300`

### Environment Variables
- `discovery.type=single-node` - Single node cluster
- `ES_JAVA_OPTS=-Xms512m -Xmx512m` - JVM heap size
- `xpack.security.enabled=false` - Security disabled
- `bootstrap.memory_lock=true` - Memory lock active
- `TZ=Europe/Istanbul` - Timezone

## 🌐 Access

You can access Elasticsearch at the following URLs:

- **HTTP API**: http://localhost:9200
- **Cluster Health**: http://localhost:9200/_cluster/health
- **Node Info**: http://localhost:9200/_nodes

## 📊 Cluster Health Check

To check cluster status:

```bash
curl -X GET "localhost:9200/_cluster/health?pretty"
```

Expected output:
```json
{
  "cluster_name" : "docker-cluster",
  "status" : "green",
  "timed_out" : false,
  "number_of_nodes" : 1,
  "number_of_data_nodes" : 1,
  "active_primary_shards" : 0,
  "active_shards" : 0,
  "relocating_shards" : 0,
  "initializing_shards" : 0,
  "unassigned_shards" : 0,
  "delayed_unassigned_shards" : 0,
  "number_of_pending_tasks" : 0,
  "number_of_in_flight_fetch" : 0,
  "task_max_waiting_in_queue_millis" : 0,
  "active_shards_percent_as_number" : 100.0
}
```

## 🔄 Docker Compose Integration

Elasticsearch has been removed from the `docker-compose.yml` file. If you want to run other services (API, PostgreSQL, Redis):

```bash
# Start only other services (without Elasticsearch)
docker-compose up -d

# Start Elasticsearch separately
./elasticsearch-manager.sh start
```

## 🛠️ Troubleshooting

### Script Not Working
If the script doesn't work on your system:

1. **Check if bash is available**:
   ```bash
   bash --version
   ```

2. **Make script executable**:
   ```bash
   chmod +x elasticsearch-manager.sh
   ```

3. **For Windows users**: Use WSL or Git Bash instead of Command Prompt

### Elasticsearch Won't Start
1. Check container logs:
   ```bash
   ./elasticsearch-manager.sh logs
   ```

2. Check system resources:
   ```bash
   docker stats elasticsearch-standalone
   ```

3. Recreate the container:
   ```bash
   ./elasticsearch-manager.sh remove
   ./elasticsearch-manager.sh start
   ```

### Port Conflict
If port 9200 is in use:
1. Stop the existing service
2. Or use a different port (by editing the script)

### Memory Issues
Make sure you have enough RAM for Elasticsearch (minimum 512MB recommended).

## 📝 Notes

- Elasticsearch may take some time to start initially (30-60 seconds)
- Data is lost when the container stops (no persistent volume used)
- Remember to enable security settings in production environment
- Use `./elasticsearch-manager.sh logs` to follow logs
- The script requires bash shell and Docker to be installed

## 🔗 Useful Links

- [Elasticsearch Docker Documentation](https://www.elastic.co/guide/en/elasticsearch/reference/current/docker.html)
- [Elasticsearch REST API](https://www.elastic.co/guide/en/elasticsearch/reference/current/rest-apis.html)
- [Elasticsearch Cluster Health API](https://www.elastic.co/guide/en/elasticsearch/reference/current/cluster-health.html) 