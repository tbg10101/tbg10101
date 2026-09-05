# Refining the workflow

`/tackle refine <feedback>` mutates this system. The feedback is about how
the *workflow* behaved, not about a particular task's code.

Procedure:

1. **Classify the feedback.** Which is it?
   - a phase's instructions were wrong or missing → edit `references/phases.md`
   - an agent behaved badly, over- or under-reached → edit that agent's
     `~/.claude/agents/tackle-*.md`
   - a gate fired too often or not enough, or an escalation was mishandled →
     edit `SKILL.md`
   - it's project-specific, not universal → edit that project's
     `.claude/tackle.md` instead, and say so
   - a whole aspect is unreviewed → add a new `tackle-review-<aspect>.md` agent and
     register it in the roster in `SKILL.md`
2. **Show the diff before applying it.** State what you're changing and why, in
   one or two lines. A workflow that silently rewrites itself is not one the
   user can trust.
3. **Change the smallest thing that fixes it.** Prefer editing an existing
   instruction over adding a new one; these files are read every run and bloat
   makes them less likely to be followed.
4. **Prune while you're in there.** If a rule is now contradicted or subsumed by
   the new one, delete it rather than leaving both.
5. Keep the same voice: imperative, terse, reasoning as lists not paragraphs.

Refinement is a local edit to `~/.claude/`. Do not commit or push it unless the
user asks.

If the feedback implies a rule the user will want in *every* project regardless
of this workflow, say so and offer to put it in `~/.claude/CLAUDE.md` instead —
that reaches more than this skill does.
