#!/usr/bin/env node
// Syncs each package.json's version into its corresponding "main" .csproj's <Version> property,
// so the NuGet package version stays in lockstep with what Changesets tracks. Assumes the
// src/<Project>/<Project>/<Project>.csproj convention already used across this repo.
import { execFileSync } from "node:child_process";
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import path from "node:path";

function git(...args) {
  return execFileSync("git", args, { encoding: "utf8" }).trim();
}

function getProjectDirs() {
  const out = git("ls-files", "src/*/package.json");
  return out.split("\n").filter(Boolean).map((p) => path.dirname(p));
}

const VERSION_TAG_RE = /<Version>[^<]*<\/Version>/;

let updated = 0;

for (const projectDir of getProjectDirs()) {
  const name = path.basename(projectDir); // e.g. "Beans.Patchable"
  const pkg = JSON.parse(readFileSync(path.join(projectDir, "package.json"), "utf8"));
  const csprojPath = path.join(projectDir, name, `${name}.csproj`);

  if (!existsSync(csprojPath)) {
    console.log(`Skipping ${name}: no ${csprojPath} found.`);
    continue;
  }

  const original = readFileSync(csprojPath, "utf8");
  const contents = VERSION_TAG_RE.test(original)
    ? original.replace(VERSION_TAG_RE, `<Version>${pkg.version}</Version>`)
    : original.replace(/(<PropertyGroup>)/, `$1\n    <Version>${pkg.version}</Version>`);

  if (contents === original) {
    console.log(`${name} already at ${pkg.version} - no change.`);
    continue;
  }

  writeFileSync(csprojPath, contents);
  updated++;
  console.log(`Set ${name} to ${pkg.version} in ${csprojPath}`);
}

console.log(`Updated ${updated} project(s).`);
