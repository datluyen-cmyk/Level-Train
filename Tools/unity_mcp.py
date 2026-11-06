#!/usr/bin/env python3
"""Small stdlib MCP HTTP client for the local Coplay Unity MCP server.

Examples:
  python Tools/unity_mcp.py tools
  python Tools/unity_mcp.py resources
  python Tools/unity_mcp.py resource mcpforunity://instances
  python Tools/unity_mcp.py call read_console '{"action":"get","count":20}'
"""
import argparse
import json
import sys
import urllib.error
import urllib.request

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8")

DEFAULT_URL = "http://127.0.0.1:8080/mcp"

class McpClient:
    def __init__(self, url):
        self.url = url
        self.session_id = None
        self.next_id = 1
        self._request("initialize", {
            "protocolVersion": "2025-03-26",
            "capabilities": {},
            "clientInfo": {"name": "unity-mcp-cli", "version": "1.0.0"},
        })
        self._notify("notifications/initialized", {})

    def _post(self, payload):
        headers = {"Content-Type": "application/json", "Accept": "application/json, text/event-stream"}
        if self.session_id:
            headers["mcp-session-id"] = self.session_id
        req = urllib.request.Request(self.url, json.dumps(payload).encode("utf-8"), headers)
        try:
            with urllib.request.urlopen(req, timeout=90) as response:
                self.session_id = response.headers.get("mcp-session-id", self.session_id)
                raw = response.read().decode("utf-8", "replace")
        except urllib.error.HTTPError as exc:
            detail = exc.read().decode("utf-8", "replace")
            raise RuntimeError(f"MCP HTTP {exc.code}: {detail}") from exc
        except urllib.error.URLError as exc:
            raise RuntimeError(f"Cannot reach {self.url}: {exc.reason}") from exc
        messages = []
        data_lines = []
        for line in raw.splitlines() + [""]:
            if not line:
                if data_lines:
                    data = "\n".join(data_lines)
                    try:
                        messages.append(json.loads(data))
                    except json.JSONDecodeError:
                        pass
                    data_lines = []
            elif line.startswith("data:"):
                data_lines.append(line[5:].lstrip())
        for message in messages:
            if "error" in message:
                raise RuntimeError(json.dumps(message["error"], indent=2))
            if "result" in message:
                return message["result"]
        return None

    def _request(self, method, params):
        request_id = self.next_id
        self.next_id += 1
        return self._post({"jsonrpc": "2.0", "id": request_id, "method": method, "params": params})

    def _notify(self, method, params):
        self._post({"jsonrpc": "2.0", "method": method, "params": params})

    def command(self, operation, argument=None):
        if operation == "tools":
            return self._request("tools/list", {})
        if operation == "resources":
            return self._request("resources/list", {})
        if operation == "resource":
            return self._request("resources/read", {"uri": argument})
        if operation == "call":
            name, sep, raw_args = argument.partition(" ")
            if not sep:
                raw_args = "{}"
            return self._request("tools/call", {"name": name, "arguments": json.loads(raw_args)})
        raise ValueError(f"Unknown operation: {operation}")

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("operation", choices=("tools", "resources", "resource", "call"))
    parser.add_argument("argument", nargs="*", help="resource URI, or tool_name followed by JSON arguments")
    parser.add_argument("--url", default=DEFAULT_URL)
    args = parser.parse_args()
    argument = " ".join(args.argument)
    if args.operation in ("resource", "call") and not argument:
        parser.error(f"{args.operation} requires an argument")
    try:
        client = McpClient(args.url)
        result = client.command(args.operation, argument)
        print(json.dumps(result, indent=2, ensure_ascii=False))
    except Exception as exc:
        print(f"unity_mcp: {exc}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())



