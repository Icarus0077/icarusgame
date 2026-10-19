CREATE TABLE `ranked_matches` (
	`id` text PRIMARY KEY NOT NULL,
	`game_id` text NOT NULL,
	`season_id` text NOT NULL,
	`player_a_id` text NOT NULL,
	`player_b_id` text NOT NULL,
	`outcome` text NOT NULL,
	`rating_a_before` integer NOT NULL,
	`rating_a_after` integer NOT NULL,
	`rating_b_before` integer NOT NULL,
	`rating_b_after` integer NOT NULL,
	`delta_a` integer NOT NULL,
	`delta_b` integer NOT NULL,
	`is_simulation` integer NOT NULL,
	`batch_id` text,
	`created_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL,
	FOREIGN KEY (`player_a_id`) REFERENCES `game_users`(`id`) ON UPDATE no action ON DELETE cascade,
	FOREIGN KEY (`player_b_id`) REFERENCES `game_users`(`id`) ON UPDATE no action ON DELETE cascade,
	CONSTRAINT "ranked_match_outcome" CHECK("ranked_matches"."outcome" IN ('a','b','draw')),
	CONSTRAINT "ranked_match_distinct_players" CHECK("ranked_matches"."player_a_id" <> "ranked_matches"."player_b_id"),
	CONSTRAINT "ranked_match_conserved_rating" CHECK("ranked_matches"."delta_a" + "ranked_matches"."delta_b" = 0)
);
--> statement-breakpoint
CREATE INDEX `idx_ranked_matches_season` ON `ranked_matches` (`game_id`,`season_id`,`created_at`);--> statement-breakpoint
CREATE TABLE `ranked_profiles` (
	`game_id` text NOT NULL,
	`season_id` text NOT NULL,
	`user_id` text NOT NULL,
	`rating` integer DEFAULT 1000 NOT NULL,
	`games` integer DEFAULT 0 NOT NULL,
	`wins` integer DEFAULT 0 NOT NULL,
	`losses` integer DEFAULT 0 NOT NULL,
	`draws` integer DEFAULT 0 NOT NULL,
	`updated_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL,
	PRIMARY KEY(`game_id`, `season_id`, `user_id`),
	FOREIGN KEY (`user_id`) REFERENCES `game_users`(`id`) ON UPDATE no action ON DELETE cascade,
	CONSTRAINT "ranked_profile_totals" CHECK("ranked_profiles"."games" = "ranked_profiles"."wins" + "ranked_profiles"."losses" + "ranked_profiles"."draws"),
	CONSTRAINT "ranked_profile_games_nonnegative" CHECK("ranked_profiles"."games" >= 0)
);
--> statement-breakpoint
CREATE TABLE `ranked_simulation_batches` (
	`id` text PRIMARY KEY NOT NULL,
	`request_json` text NOT NULL,
	`report_json` text NOT NULL,
	`created_at` text DEFAULT CURRENT_TIMESTAMP NOT NULL
);
