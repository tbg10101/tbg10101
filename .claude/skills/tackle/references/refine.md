# Refining the workflow

`/tackle refine <feedback>` changes this workflow. The feedback is about how the
workflow behaved, not about a task's code.

1. **Classify** and pick the file:
   - a phase's instructions wrong or missing → `references/phases.md`
   - an agent over- or under-reached → `~/.claude/agents/tackle-*.md`
   - a gate fired too often or too rarely, or an escalation went wrong →
     `SKILL.md`
   - project-specific → that project's `.claude/tackle.md`; say so
   - an unreviewed aspect → a new `tackle-review-<aspect>.md` agent, registered
     in the `SKILL.md` roster
2. **Show the diff before applying it**, with a line or two on why. A workflow
   that silently rewrites itself can't be trusted.
3. **Change the smallest thing.** Edit an existing instruction before adding
   one; these files are read every run, and bloat gets them ignored.
4. **Prune.** Delete rules the change contradicts or subsumes.
5. **Match the voice:** imperative, terse, reasoning as lists.

Refinement is a local edit to `~/.claude/`. Commit or push only if asked.

If the feedback is a rule the user would want in every project, offer to put it
in `~/.claude/CLAUDE.md` instead.
