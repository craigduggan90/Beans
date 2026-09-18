#!/usr/bin/env node
// Synthesizes .changeset/*.md files from conventional commits in a given range, one per
// (commit, affected package) pair. Mirrors semantic-release's default Angular preset: feat -> minor,
// fix/perf -> patch, a "!" or BREAKING CHANGE footer -> major, everything else is skipped.
import { execFileSync } from "node:child_process";
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import path from "node:path";

const [, , fromSha, toSha] = process.argv;

if (!fromSha || !toSha) {
  console.error("Usage: create-changesets-from-commits.mjs <fromSha> <toSha>");
  process.exit(1);
}

const TYPE_TO_BUMP = {
  feat: "minor",
  fix: "patch",
  perf: "patch",
};

const COMMIT_HEADER_RE = /^(?<type>[a-z]+)(?:\((?<scope>[^)]+)\))?(?<breaking>!)?:\s*(?<subject>.+)$/;
const BREAKING_FOOTER_RE = /BREAKING[ -]CHANGE:/;

function git(...args) {
  return execFileSync("git", args, { encoding: "utf8" }).trim();
}

function getCommitShas(from, to) {
  const out = git("rev-list", `${from}..${to}`);
  return out ? out.split("\n").reverse() : []; // oldest first, for readable log output
}

function getCommitMessage(sha) {
  return git("log", "-1", "--format=%B", sha);
}

function getChangedFiles(sha) {
  const out = git("show", "--name-only", "--pretty=format:", sha);
  return out.split("\n").filter(Boolean);
}

function getProjectDirs() {
  const out = git("ls-files", "src/*/package.json");
  return out.split("\n").filter(Boolean).map((p) => path.dirname(p));
}

function readPackageName(projectDir) {
  const pkg = JSON.parse(readFileSync(path.join(projectDir, "package.json"), "utf8"));
  return pkg.name;
}

const projects = getProjectDirs();
const shas = getCommitShas(fromSha, toSha);

if (!existsSync(".changeset")) mkdirSync(".changeset");

let changesetCount = 0;

for (const sha of shas) {
  const message = getCommitMessage(sha);
  const [headerLine, ...rest] = message.split("\n");
  const match = headerLine.match(COMMIT_HEADER_RE);

  if (!match) continue; // not a conventional commit - skip

  const { type, breaking, subject } = match.groups;
  const isBreaking = Boolean(breaking) || BREAKING_FOOTER_RE.test(rest.join("\n"));
  const bump = isBreaking ? "major" : TYPE_TO_BUMP[type];

  if (!bump) continue; // type doesn't warrant a release (docs/chore/ci/test/etc.)

  const changedFiles = getChangedFiles(sha);
  const affectedProjects = projects.filter((projectDir) =>
    changedFiles.some((file) => file.startsWith(`${projectDir}/`)),
  );

  for (const projectDir of affectedProjects) {
    const name = readPackageName(projectDir);
    const id = `auto-${sha.slice(0, 12)}-${name.replace(/[^a-z0-9]/gi, "-").toLowerCase()}`;
    const body = `---\n"${name}": ${bump}\n---\n\n${subject}\n`;
    writeFileSync(path.join(".changeset", `${id}.md`), body);
    changesetCount++;
    console.log(`Created changeset for ${name} (${bump}): ${subject}`);
  }
}

console.log(`Created ${changesetCount} changeset(s).`);
