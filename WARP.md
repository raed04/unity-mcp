# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

Unity MCP is a bridge that connects AI assistants (Claude, Cursor, etc.) to Unity Editor via the Model Context Protocol (MCP). It consists of:

1. **Unity Bridge** (`UnityMcpBridge/`) - Unity package that runs inside the Editor
2. **Python MCP Server** (`UnityMcpBridge/UnityMcpServer~/src/`) - Python server that communicates between Unity and MCP clients
3. **Development Tools** - Scripts for testing and development deployment

## Architecture

### Two-Component System
- **Unity Package**: Contains C# editor scripts that expose Unity functionality via TCP socket
- **Python Server**: MCP-compliant server that translates between MCP protocol and Unity Bridge socket protocol
- **Communication**: Unity Bridge listens on TCP socket, Python server connects and translates MCP calls to Unity API calls

### Key Python Modules
- `server.py` - Main MCP server using FastMCP framework with telemetry and connection management
- `unity_connection.py` - Handles TCP communication with Unity Bridge
- `tools/` directory - Contains all MCP tool implementations (manage_script, manage_scene, etc.)
- `telemetry.py` - Anonymous usage analytics with privacy-focused opt-out support

### MCP Tools Available
- `manage_script` - C# script CRUD operations with advanced text editing and validation
- `manage_scene` - Scene management (load, save, create, hierarchy)
- `manage_gameobject` - GameObject creation, modification, component operations
- `manage_asset` - Asset operations (import, create, modify, delete)
- `manage_shader` - Shader CRUD operations
- `manage_editor` - Editor state control and queries
- `manage_menu_item` - Unity menu item execution and discovery
- `read_console` - Console message reading and filtering
- `apply_text_edits` - Precise text editing with precondition hashes
- `script_apply_edits` - Structured C# method/class edits with safer boundaries
- `validate_script` - Script validation (basic/standard/strict levels)

## Common Development Commands

### Testing
```bash
# Run all tests
python -m pytest tests/ -v

# Run specific test file
python -m pytest tests/test_script_editing.py -v

# Run tests with coverage
python -m pytest tests/ --cov=. --cov-report=html

# Test telemetry functionality
cd UnityMcpBridge/UnityMcpServer~/src
python test_telemetry.py
```

### Development Deployment
```bash
# Deploy development code to Unity installation (Windows)
deploy-dev.bat

# Restore original files from backup
restore-dev.bat

# Switch package sources between upstream/remote/local
python mcp_source.py --choice 3  # Use local workspace
```

### Python Server Development
```bash
# Run server directly for testing
cd UnityMcpBridge/UnityMcpServer~/src
uv run server.py

# Install dependencies
uv install

# Run with telemetry disabled
DISABLE_TELEMETRY=true uv run server.py
```

### Stress Testing
```bash
# Run MCP bridge stress test with multiple concurrent clients
python tools/stress_mcp.py --duration 60 --clients 8 --unity-file "TestProjects/UnityMCPTests/Assets/Scripts/LongUnityScriptClaudeTest.cs"
```

### Utilities
```bash
# Compact large tool_result blobs in conversation logs
python prune_tool_results.py < reports/claude-execution-output.json > reports/claude-execution-output.pruned.json

# Test Unity socket framing
python test_unity_socket_framing.py
```

## Code Architecture Guidelines

### Unity Bridge (C#)
- Lives in Unity Editor as package
- Exposes Unity API via TCP socket server
- Uses JSON-RPC style communication
- Handles script compilation, asset management, scene operations
- Must be compatible with Unity 2021.3 LTS+

### Python Server Architecture
- Built on FastMCP framework for MCP protocol compliance
- Async/await pattern for non-blocking operations  
- Connection pooling and retry logic for Unity Bridge communication
- Telemetry integration with privacy-first anonymous collection
- Tool registration system in `tools/__init__.py`
- Rotating file logs in `~/Library/Application Support/UnityMCP/Logs/`

### Key Patterns
- **Tool Implementation**: Each tool in `tools/` directory implements specific Unity functionality
- **Error Handling**: Graceful degradation with informative error messages
- **Precondition Checking**: SHA256 hashes for atomic file operations
- **Connection Management**: Auto-discovery of Unity Bridge port via status JSON files
- **Framing Protocol**: Custom framing for reliable TCP message delivery

### Telemetry Design
- Anonymous UUIDs only, no personal data
- Opt-out via `DISABLE_TELEMETRY=true` environment variable  
- Non-blocking background transmission
- Privacy-focused data collection documented in TELEMETRY.md
- Thread-safe implementation with graceful failure handling

## File Structure Notes
- `UnityMcpBridge/` - Unity package root with package.json
- `UnityMcpBridge/UnityMcpServer~/` - Python server bundled with Unity package
- `tests/` - Python test suite using pytest
- `tools/stress_mcp.py` - Concurrent client stress testing utility
- Development scripts are in repository root (deploy-dev.bat, mcp_source.py)
- Reports and artifacts go in `reports/` directory

## Development Workflow
1. Make changes to Unity Bridge C# code or Python server code
2. Deploy using `deploy-dev.bat` (creates timestamped backups automatically)
3. Restart Unity Editor to load new Bridge code  
4. Restart MCP clients to use new Server code
5. Test functionality through MCP client or direct server testing
6. Use `restore-dev.bat` to rollback if needed
7. Run test suite to ensure no regressions

## Important Implementation Details
- Unity Bridge communicates via TCP socket with custom framing protocol
- Python server handles MCP protocol translation and tool routing
- Script validation supports multiple levels: basic (structural) to strict (Roslyn-based)
- Text editing operations use precondition SHA256 hashes to prevent conflicts
- Connection discovery uses JSON status files in `~/.unity-mcp/`
- All file paths in tools are relative to Unity project's Assets/ folder unless absolute
- Menu item operations support caching with refresh capabilities
