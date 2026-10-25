import { describe, it, expect } from "vitest";
import Ajv from "ajv";
import addFormats from "ajv-formats";
import fs from "node:fs";
import path from "node:path";

describe("JSON Schema Validation Tests", () => {
  const ajv = new Ajv({ allErrors: true, strict: false });
  addFormats(ajv);

  const rootDir = path.resolve(__dirname, "../..");

  it("validates golden-hud-tree.json against schema/ui-tree.schema.json", () => {
    const schemaPath = path.join(rootDir, "schema/ui-tree.schema.json");
    const dataPath = path.join(rootDir, "tests/fixtures/golden-hud-tree.json");

    const schema = JSON.parse(fs.readFileSync(schemaPath, "utf-8"));
    const data = JSON.parse(fs.readFileSync(dataPath, "utf-8"));

    const validate = ajv.compile(schema);
    const valid = validate(data);
    expect(validate.errors).toBeNull();
    expect(valid).toBe(true);
  });

  it("validates golden-hud-validation.json against schema/ui-validation.schema.json", () => {
    const schemaPath = path.join(rootDir, "schema/ui-validation.schema.json");
    const dataPath = path.join(rootDir, "tests/fixtures/golden-hud-validation.json");

    const schema = JSON.parse(fs.readFileSync(schemaPath, "utf-8"));
    const data = JSON.parse(fs.readFileSync(dataPath, "utf-8"));

    const validate = ajv.compile(schema);
    const valid = validate(data);
    expect(validate.errors).toBeNull();
    expect(valid).toBe(true);
  });

  it("validates golden-diff.json against schema/ui-diff.schema.json", () => {
    const schemaPath = path.join(rootDir, "schema/ui-diff.schema.json");
    const dataPath = path.join(rootDir, "tests/fixtures/golden-diff.json");

    const schema = JSON.parse(fs.readFileSync(schemaPath, "utf-8"));
    const data = JSON.parse(fs.readFileSync(dataPath, "utf-8"));

    const validate = ajv.compile(schema);
    const valid = validate(data);
    expect(validate.errors).toBeNull();
    expect(valid).toBe(true);
  });

  it("validates .uicontext.json against schema/config.schema.json", () => {
    const schemaPath = path.join(rootDir, "schema/config.schema.json");
    const dataPath = path.join(rootDir, ".uicontext.json");

    const schema = JSON.parse(fs.readFileSync(schemaPath, "utf-8"));
    const data = JSON.parse(fs.readFileSync(dataPath, "utf-8"));

    const validate = ajv.compile(schema);
    const valid = validate(data);
    expect(validate.errors).toBeNull();
    expect(valid).toBe(true);
  });
});
