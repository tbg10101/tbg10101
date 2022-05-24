# Dependencies:
#   - bash 4.4.12(3)
#   - git 2.35.2
#   - coreutils 8.32
#   - 

# If not running interactively, don't do anything
[[ "$-" != *i* ]] && return

# Sources
source ~/.bash_colors

# Aliases
alias ll='ls -al --color=auto'
alias l='ll'
alias cd..='cd ..'

# Functions
# TODO

# PS1 (shamelessly stolen from somewhere but I can't remember any more)
function timer_now() {
    date +%s%N
}

function timer_start() {
    commandStartTime=${commandStartTime:-$(timer_now)}
}

function timer_stop() {
    local delta_us=$((($(timer_now) - $commandStartTime) / 1000))
    local us=$((delta_us % 1000))
    local ms=$(((delta_us / 1000) % 1000))
    local s=$(((delta_us / 1000000) % 60))
    local m=$(((delta_us / 60000000) % 60))
    local h=$((delta_us / 3600000000))
    # Goal: always show around 3 digits of accuracy
    if ((h > 0)); then timer_show=${h}h${m}m
    elif ((m > 0)); then timer_show=${m}m${s}s
    elif ((s >= 10)); then timer_show=${s}.$((ms / 100))s
    elif ((s > 0)); then timer_show=${s}.$(printf %03d $ms)s
    elif ((ms >= 100)); then timer_show=${ms}ms
    elif ((ms > 0)); then timer_show=${ms}.$((us / 100))ms
    else timer_show=${us}us
    fi
    unset commandStartTime
}

function set_bash_prompt() {
    previous_cmd_status=$? # Must come first!
    timer_stop
    current_git_branch=`git rev-parse --abbrev-ref HEAD 2> /dev/null`
    
    cmdNumberPS1="$DEFAULT_STYLE[$FORE_GRAY\\#$DEFAULT_STYLE]"
    dateTimePS1=" [$FORE_CYAN\\D{%F} \\t$DEFAULT_STYLE]"
    userHostPS1=" [$FORE_GREEN\u$DEFAULT_STYLE@$FORE_YELLOW\h$DEFAULT_STYLE]"
    workingDirectoryPS1=" [\w]"
    if [ -n "$current_git_branch" ]; then
        working_dir=`(cd -P .; pwd)`
        current_git_repo_path=`git rev-parse --show-toplevel HEAD 2> /dev/null`
        
        parentOfGitRoot=$(dirname "$current_git_repo_path")/
        parentOfGitRootLength=${#parentOfGitRoot}
        workingDirLength=${#working_dir}
        gitDirectories=${working_dir:$parentOfGitRootLength:$workingDirLength}

        workingDirectoryPS1="[$parentOfGitRoot$FORE_MAGENTA_LIGHT$gitDirectories$DEFAULT_STYLE]"
    fi
    previousCmdTimePS1=" [${timer_show}]"
    gitBranchPS1=''
    if [ -n "$current_git_branch" ]; then
        gitBranchPS1="[$FORE_MAGENTA_LIGHT${current_git_branch}$DEFAULT_STYLE]"
    fi
    previousCmdStatusPS1=''
    if [ "$previous_cmd_status" -ne 0 ]; then
        previousCmdStatusPS1=" [$FORE_RED$previous_cmd_status$DEFAULT_STYLE]"
    fi
    
    export PS1="${cmdNumberPS1}${dateTimePS1}${userHostPS1}${workingDirectoryPS1}${gitBranchPS1}${previousCmdTimePS1}${previousCmdStatusPS1}\n$ "
}

trap timer_start DEBUG
PROMPT_COMMAND=set_bash_prompt

# Message of the Day
# TODO
