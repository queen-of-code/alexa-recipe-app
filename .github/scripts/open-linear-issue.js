/**
 * Create a Linear issue from a newly opened GitHub issue, comment the
 * Linear URL, and close the GitHub issue.
 *
 * Requires the LINEAR_API_KEY secret (a Linear personal API key).
 */

const LINEAR_URL = 'https://api.linear.app/graphql'
const TEAM_KEY = 'QUE'
const PROJECT_NAME = 'Alexa Recipe App'

async function linear(token, query, variables) {
  const response = await fetch(LINEAR_URL, {
    method: 'POST',
    headers: {
      Authorization: token,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ query, variables }),
  })

  const payload = await response.json()
  if (!response.ok || payload.errors) {
    const detail = payload.errors ? JSON.stringify(payload.errors) : response.statusText
    throw new Error(`Linear API request failed: ${detail}`)
  }
  return payload.data
}

function pickTriageState(states) {
  return (
    states.find((state) => state.type === 'triage') ||
    states.find((state) => state.name.toLowerCase() === 'triage') ||
    null
  )
}

function githubIssueMarker(issue) {
  return issue.html_url
}

function buildDescription(issue) {
  const body = (issue.body || '').trim()
  const lines = []
  if (body) {
    lines.push(body, '')
  }
  lines.push(
    '---',
    `Mirrored from GitHub issue #${issue.number} by @${issue.user.login}: ${githubIssueMarker(issue)}`
  )
  return lines.join('\n')
}

function hasMirrorComment(comments) {
  return comments.some(
    (comment) =>
      comment.user?.login === 'github-actions[bot]' &&
      comment.body?.includes('https://linear.app/')
  )
}

function findMirroredLinearIssue(issues, marker) {
  return issues.find((candidate) => candidate.description?.includes(marker)) ?? null
}

async function findExistingMirror(token, teamId, issue) {
  const marker = githubIssueMarker(issue)
  const data = await linear(
    token,
    `query($teamId: ID!, $marker: String!) {
      issues(
        filter: {
          team: { id: { eq: $teamId } }
          description: { contains: $marker }
        }
        first: 5
      ) {
        nodes { identifier url description }
      }
    }`,
    { teamId, marker }
  )

  return findMirroredLinearIssue(data.issues.nodes, marker)
}

async function openLinearIssue({ github, context, core }) {
  const rawKey = process.env.LINEAR_API_KEY
  if (!rawKey || !rawKey.trim()) {
    core.setFailed('Set the LINEAR_API_KEY repository secret to a Linear personal API key.')
    return
  }
  const token = rawKey.replace(/^Bearer\s+/i, '').trim()

  const { owner, repo } = context.repo
  const issue = context.payload.issue
  const issueNumber = issue.number

  const { data: comments } = await github.rest.issues.listComments({
    owner,
    repo,
    issue_number: issueNumber,
    per_page: 100,
  })
  if (hasMirrorComment(comments)) {
    core.info(`Issue #${issueNumber} already links a Linear ticket. Skipping.`)
    return
  }

  const data = await linear(
    token,
    `query {
      teams(first: 50) {
        nodes {
          id
          key
          states {
            nodes { id name type }
          }
        }
      }
      projects(first: 50) {
        nodes { id name }
      }
    }`
  )

  const team = data.teams.nodes.find((candidate) => candidate.key === TEAM_KEY)
  if (!team) {
    throw new Error(`Linear team ${TEAM_KEY} was not found for this API key.`)
  }

  const triage = pickTriageState(team.states.nodes)
  if (!triage) {
    throw new Error(`Linear team ${TEAM_KEY} has no Triage workflow state.`)
  }

  const project = data.projects.nodes.find((candidate) => candidate.name === PROJECT_NAME)
  if (!project) {
    throw new Error(`Linear project "${PROJECT_NAME}" was not found for this API key.`)
  }

  let linearIssue = await findExistingMirror(token, team.id, issue)
  if (linearIssue) {
    core.info(
      `Found existing Linear mirror ${linearIssue.identifier}: ${linearIssue.url}`
    )
  } else {
    const created = await linear(
      token,
      `mutation($input: IssueCreateInput!) {
        issueCreate(input: $input) {
          success
          issue { identifier url }
        }
      }`,
      {
        input: {
          teamId: team.id,
          projectId: project.id,
          stateId: triage.id,
          title: issue.title,
          description: buildDescription(issue),
        },
      }
    )

    if (!created.issueCreate.success || !created.issueCreate.issue) {
      throw new Error('Linear issueCreate did not succeed.')
    }

    linearIssue = created.issueCreate.issue
    core.info(`Created ${linearIssue.identifier}: ${linearIssue.url}`)
  }

  const { identifier, url } = linearIssue

  await github.rest.issues.createComment({
    owner,
    repo,
    issue_number: issueNumber,
    body: `Tracked in Linear as ${identifier}: ${url}\n\nClosing this GitHub issue. Continue the work on the Linear ticket.`,
  })

  await github.rest.issues.update({
    owner,
    repo,
    issue_number: issueNumber,
    state: 'closed',
    state_reason: 'completed',
  })
}

module.exports = {
  openLinearIssue,
  pickTriageState,
  buildDescription,
  githubIssueMarker,
  hasMirrorComment,
  findMirroredLinearIssue,
}
