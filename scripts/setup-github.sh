#!/usr/bin/env bash
# Creates the GitHub repo, labels and milestones for InterviewCopilot.
# Run it yourself after the first local commit:  bash scripts/setup-github.sh
# Requires: gh (authenticated as ukez15). It pushes main once, when you confirm.
set -euo pipefail

REPO="ukez15/interview-copilot"

if ! gh repo view "$REPO" >/dev/null 2>&1; then
  gh repo create "$REPO" --public --source . --remote origin \
    --description "Free, open-source real-time AI interview and conversation assistant (local Whisper + Ollama)"
fi

read -r -p "Push the local main branch to $REPO now? [y/N] " answer
if [[ "$answer" =~ ^[Yy]$ ]]; then
  git push -u origin main
fi

label() { gh label create "$1" --repo "$REPO" --color "$2" --description "${3:-}" --force; }

for layer in backend frontend desktop extension database auth ci-cd documentation; do
  label "layer:$layer" 0075ca
done
label "type:feature" a2eeef; label "type:bug" d73a4a; label "type:task" e4e669; label "type:spike" ededed; label "type:epic" 5319e7
label "priority:critical" b60205; label "priority:high" d93f0b; label "priority:medium" fbca04; label "priority:low" 0e8a16
for module in auth sessions documents audio transcription copilot practice transcripts providers overlay settings; do
  label "module:$module" c5def5
done

milestone() {
  gh api "repos/$REPO/milestones" -f title="$1" -f description="$2" >/dev/null 2>&1 || echo "milestone '$1' already exists"
}
milestone "v0.1 Foundation" "Domain, schema, contracts, CI and governance (scaffold)"
milestone "v0.2 Core" "Live transcription and answers end to end with local models"
milestone "v0.3 Supporting" "Practice mode, history/PDF, providers, settings"
milestone "v0.4 Integration" "Meeting detection, browser companion, reliability, latency"
milestone "v1.0 Release" "Installers, docs, first public release"

echo "Done. Next: bash scripts/create-issues.sh"
