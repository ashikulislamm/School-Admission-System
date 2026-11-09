# AI usage

## Tool

I used one AI tool for this project: Claude Code (Anthropic), inside VS Code. Nothing else.

## What I used it for

- Planning. I had it help me write down the requirements, the architecture and the rules in the project docs, and to work out how to stop two people grabbing the last seat at the same time.
- Backend. The database model, the eight endpoints, the seat-locking logic, the expiry background job and the error handling.
- Backend tests. The concurrency test (300 people, 120 seats), expiry, ownership, validation, and the two-backends-one-database test.
- Frontend. The screens, the countdown that flips to EXPIRED by itself, the error messages, and the light theme.
- Frontend tests, the two Dockerfiles, and the compose file.
- First drafts of the README, ASSUMPTIONS and this file. I read and corrected them.

I made every git commit myself. The AI only suggested commit messages. I ran the app in Docker and clicked through it in the browser to check it did what the brief asks.

## Where the AI got things wrong

These are the real problems, and how each one showed up.

1. **SSL check on the connection string.** It used `ContainsKey("SSL Mode")` to see if an SSL option was set. That method says yes for every key, so SSL would never have been forced for a hosted database. A unit test failed (4 failures). It now reads the raw string instead.

2. **Mismatched package versions.** The test project pulled EF Core 10.0.4 while the API used 10.0.14, which gave build warnings. I saw it in the build output and pinned both to 10.0.14.
3. **SQL turned into a parameter.** It put a SQL snippet inside an interpolated `SqlQuery` call. EF treated it as a parameter, so the status field came back as SQL text. I noticed it with a quick curl test and switched to `SqlQueryRaw`.
4. **JSON was not strict.** It assumed the API would reject `"totalSeats": "5"`. It accepted it and returned 201. My validation tests caught it, and I turned on strict number handling.

5. **Test files that were never written.** It used a shell command with apostrophes in it to create several test files. The command failed silently and nothing was written. I only noticed because the test run found no tests.
6. **A test that hung.** The application page test never finished because the page waits on a promise while the test had faked the clock. Every test in that file failed. The fix was to pass an already-resolved promise and fake only the clock and the interval.
7. **A type error in that same test.** The page needs a `searchParams` prop that the test left out. The tests passed but `tsc` and `next build` failed, and that would also have broken the Docker build. Fixed by passing the prop.
8. **Apply looked missing.** The class list showed Apply greyed out until a student ID was typed, with only a faint grey hint. I opened the app and could not find the button, so I told the AI. It made the hint clear and dark.