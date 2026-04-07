---
name: playwright-testing
description: "Test the buckeye-lending frontend with Playwright MCP. Use when: Playwright test, e2e test, frontend test, visual verification, screenshot test, click-through test, regression test, component interaction test, UI test, browser test."
argument-hint: "Describe what to test (e.g., 'verify the loan form submits correctly')"
---

# Playwright Frontend Testing

Test the **buckeye-lending** React + Vite frontend using Playwright MCP tools. Covers visual verification, click-through user flows, regression checks, and component interaction testing.

## Prerequisites

Before running any test, confirm:

1. **Backend API** — The .NET backend must be running (`buckeye-lending/backend/`).
2. **Frontend dev server** — Ask the user whether `npm run dev` is already running in `buckeye-lending/frontend/`. If not, start it as a background process and wait for it to be ready on `http://localhost:5173`.

## App Overview

| Route    | Component   | Purpose                       |
| -------- | ----------- | ----------------------------- |
| `/`      | `Dashboard` | Main loan application list    |
| `/apply` | `LoanForm`  | Submit a new loan application |

Key components: `LoanApplicationCard`, `LoanApplicationList`, `ActionButtons`, `QuantitySelector`.

State is managed via `LoanProvider` context with a reducer pattern.

## Available MCP Tools

| Tool                  | Use For                                              |
| --------------------- | ---------------------------------------------------- |
| `open_browser_page`   | Launch a new browser page                            |
| `navigate_page`       | Go to a URL                                          |
| `read_page`           | Read current page content/DOM                        |
| `screenshot_page`     | Capture a screenshot                                 |
| `click_element`       | Click buttons, links, elements                       |
| `type_in_page`        | Type into input fields                               |
| `hover_element`       | Hover over elements                                  |
| `drag_element`        | Drag-and-drop interactions                           |
| `handle_dialog`       | Accept/dismiss alerts and dialogs                    |
| `run_playwright_code` | Run arbitrary Playwright code for complex assertions |

## Procedure

### Step 1 — Confirm Environment

Ask the user:

- Is the **backend API** running?
- Is the **frontend dev server** running on `http://localhost:5173`?

If the dev server is not running, start it:

```
cd buckeye-lending/frontend && npm run dev
```

(Run as a background process.)

### Step 2 — Open the App

1. Use `open_browser_page` to launch a browser.
2. Use `navigate_page` to go to `http://localhost:5173`.
3. Use `screenshot_page` to capture the initial state.
4. Save the screenshot to `buckeye-lending/tests/` with a descriptive name.

### Step 3 — Execute Tests by Type

Choose the appropriate test type based on the user's request:

#### Visual Verification / Screenshots

1. Navigate to the target route.
2. Wait for content to load — use `read_page` to confirm expected elements are present.
3. Use `screenshot_page` to capture the current state.
4. Save screenshots to `buckeye-lending/tests/` with names like `dashboard-initial.png`, `loan-form-empty.png`.
5. If doing a regression check, compare against a previous screenshot and report differences.

#### Click-Through User Flows

1. Navigate to the starting page.
2. Use `read_page` to identify interactive elements.
3. Execute the flow step-by-step using `click_element`, `type_in_page`, and `navigate_page`.
4. After each significant action, use `read_page` to verify the expected state change occurred.
5. Capture screenshots at key checkpoints.
6. Report pass/fail for each step.

Example flow — **Submit a Loan Application**:

1. Navigate to `/apply`.
2. Fill in form fields using `type_in_page`.
3. Click the submit button using `click_element`.
4. Verify redirect or success state via `read_page`.
5. Screenshot the result.

#### Regression Checks

1. Capture a "before" screenshot of the target page/component.
2. Perform the action or code change being tested.
3. Capture an "after" screenshot.
4. Use `read_page` to compare DOM state before and after.
5. Report any visual or structural differences.

#### Component Interaction Testing

1. Navigate to the page containing the target component.
2. Use `read_page` to locate the component's elements.
3. Interact with the component (`click_element`, `type_in_page`, `hover_element`).
4. After each interaction, use `read_page` to verify the component updated correctly.
5. Test edge cases (empty inputs, rapid clicks, boundary values).

### Step 4 — Generate .spec.ts File (When Requested)

After successfully running a test flow via MCP, generate a corresponding Playwright `.spec.ts` file in `buckeye-lending/frontend/tests/`.

The generated test file should:

- Import from `@playwright/test`.
- Mirror the steps executed via MCP.
- Use proper Playwright locators (`getByRole`, `getByText`, `getByLabel`, `getByTestId`).
- Include meaningful `test.describe` and `test` blocks.
- Add assertions with `expect()` for each verification point.

File naming: `<feature>.spec.ts` (e.g., `loan-form-submit.spec.ts`, `dashboard-load.spec.ts`).

### Step 5 — Report Results

Provide a summary in chat:

```
## Test Results

| Step | Action | Result |
|------|--------|--------|
| 1 | Navigate to / | ✅ Pass |
| 2 | Verify dashboard loads | ✅ Pass |
| 3 | Click "Apply" link | ✅ Pass |
| ... | ... | ... |

**Screenshots saved to:** `buckeye-lending/tests/`
**Spec file generated:** `buckeye-lending/frontend/tests/<name>.spec.ts`
```

## Known Gotchas

### screenshot_page savePath does NOT persist to disk

The `screenshot_page` tool's `savePath` parameter displays the image in chat but **does not reliably save the file to the filesystem**. To actually save screenshots, use `run_playwright_code`:

```js
await page.screenshot({
  path: "/absolute/path/to/buckeye-lending/tests/dashboard-initial.png",
  fullPage: true,
});
```

Always verify with `list_dir` that the file was created after saving.

### Dashboard has a loading state

After navigating to `/`, the first `read_page` may return `"Loading loan applications…"` while the API call completes. Always call `read_page` a second time to confirm data has loaded before asserting on card content or taking screenshots.

## Reusable Playwright Snippets

### Save a full-page screenshot to disk

```js
// Use with run_playwright_code — replace pageId and path as needed
await page.screenshot({
  path: "/absolute/path/to/buckeye-lending/tests/<name>.png",
  fullPage: true,
});
```

### Wait for dashboard cards to load before screenshotting

```js
// Waits for at least one h3 (applicant name) to appear, meaning cards have rendered
await page.waitForSelector("h3", { timeout: 5000 });
await page.screenshot({
  path: "/absolute/path/to/buckeye-lending/tests/dashboard-loaded.png",
  fullPage: true,
});
```

## Error Handling

- If a page fails to load, retry once after 3 seconds.
- If an element is not found, use `read_page` to dump the current DOM and diagnose.
- If the dev server is not responding, prompt the user to check it.
- Always capture a screenshot on failure for debugging.
- **Always use `run_playwright_code` with `page.screenshot()` to save files** — do not rely on `screenshot_page` `savePath`.

## Files

- **Screenshots**: `buckeye-lending/tests/`
- **Generated specs**: `buckeye-lending/frontend/tests/`
