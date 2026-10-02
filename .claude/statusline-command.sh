#!/bin/bash
# Claude Code status line: folder, git branch (* if dirty), model, effort, context usage circle, rate limits
input=$(cat)

model=$(echo "$input" | jq -r '.model.display_name')
effort=$(echo "$input" | jq -r '.effort.level // empty')
used=$(echo "$input" | jq -r '.context_window.used_percentage // empty')
dir=$(echo "$input" | jq -r '.workspace.current_dir // .cwd // empty')

parts=()

GREY=$'\033[38;5;250m'

# Wraps $2 in a colour for usage percentage $1: white 25+, amber 50+, red 75+, else base grey
usage_color() {
  local c=""
  if [ "$1" -ge 75 ]; then c=196
  elif [ "$1" -ge 50 ]; then c=214
  elif [ "$1" -ge 25 ]; then c=255
  fi
  if [ -n "$c" ]; then printf '\033[38;5;%sm%s%s' "$c" "$2" "$GREY"; else printf '%s' "$2"; fi
}
[ -n "$dir" ] && parts+=("$(basename "$dir")")
[ -n "$effort" ] && model="$model $effort"

# --no-optional-locks avoids contending with concurrent git commands for index.lock
branch=""
[ -n "$dir" ] && branch=$(git -C "$dir" --no-optional-locks symbolic-ref --short -q HEAD 2>/dev/null \
  || git -C "$dir" --no-optional-locks rev-parse --short HEAD 2>/dev/null)
if [ -n "$branch" ]; then
  [ -n "$(git -C "$dir" --no-optional-locks status --porcelain 2>/dev/null | head -1)" ] && branch="${branch}*"
  parts+=("$branch")
fi

if [ -n "$used" ]; then
  pct=$(printf '%.0f' "$used")
  # Nearest quarter: 0-12 ○, 13-37 ◔, 38-62 ◑, 63-87 ◕, 88+ ●
  circles=(○ ◔ ◑ ◕ ●)
  idx=$(( (pct + 12) / 25 ))
  [ "$idx" -gt 4 ] && idx=4

  parts+=("$model" "$(usage_color "$pct" "${circles[$idx]} ${pct}%")")
else
  parts+=("$model")
fi

# Subscription rate limits; each window may be absent
rate_limit() {
  local used resets
  used=$(echo "$input" | jq -r ".rate_limits.$1.used_percentage // empty")
  resets=$(echo "$input" | jq -r ".rate_limits.$1.resets_at // empty")
  [ -z "$used" ] && return
  local pct text
  pct=$(printf '%.0f' "$used")
  text=$(usage_color "$pct" "$2 ${pct}%")
  [ -n "$resets" ] && text="$text ($3$(date -r "$resets" "+$4"))"
  parts+=("$text")
}
rate_limit five_hour 5h "↻ " "%H:%M"
rate_limit seven_day 7d "" "%a"

line=""
for p in "${parts[@]}"; do
  [ -n "$line" ] && line="$line · "
  line="$line$p"
done

printf '%s%s\033[0m' "$GREY" "$line"
