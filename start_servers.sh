#!/bin/bash
# tmux is required for this to work

tmux new-session -d -s fastapi_template 'cd backend && source .venv/bin/activate && task start'
tmux split-window -h -t fastapi_template 'cd frontend && bun run dev'
tmux attach -t fastapi_template