const assert = require('assert')
const { pickTriageState, buildDescription } = require('./open-linear-issue')

const triage = pickTriageState([
  { id: 'backlog', name: 'Backlog', type: 'backlog' },
  { id: 'triage-id', name: 'Triage', type: 'triage' },
])
assert.strictEqual(triage.id, 'triage-id')

const byName = pickTriageState([{ id: 'named', name: 'Triage', type: 'unstarted' }])
assert.strictEqual(byName.id, 'named')

assert.strictEqual(pickTriageState([{ id: 'x', name: 'Plan', type: 'unstarted' }]), null)

const description = buildDescription({
  number: 12,
  body: 'Add a photo',
  user: { login: 'octocat' },
  html_url: 'https://github.com/queen-of-code/alexa-recipe-app/issues/12',
})
assert.match(description, /Add a photo/)
assert.match(description, /#12 by @octocat/)
assert.match(description, /issues\/12/)

const empty = buildDescription({
  number: 3,
  body: null,
  user: { login: 'octocat' },
  html_url: 'https://github.com/queen-of-code/alexa-recipe-app/issues/3',
})
assert.doesNotMatch(empty, /^null/)
assert.match(empty, /#3 by @octocat/)

console.log('open-linear-issue tests passed')
