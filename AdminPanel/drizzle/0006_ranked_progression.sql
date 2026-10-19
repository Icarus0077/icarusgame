CREATE TABLE `ranked_rewards` (
	`game_id` text NOT NULL,
	`user_id` text NOT NULL,
	`scope` text NOT NULL,
	`highest_step` integer NOT NULL,
	`amount` integer NOT NULL,
	`match_id` text NOT NULL,
	`created_at` text NOT NULL,
	PRIMARY KEY(`game_id`, `user_id`, `scope`, `match_id`),
	FOREIGN KEY (`user_id`) REFERENCES `game_users`(`id`) ON UPDATE no action ON DELETE cascade,
	FOREIGN KEY (`match_id`) REFERENCES `ranked_matches`(`id`) ON UPDATE no action ON DELETE cascade
);
--> statement-breakpoint
CREATE TABLE `ranked_wallets` (
	`game_id` text NOT NULL,
	`user_id` text NOT NULL,
	`scope` text NOT NULL,
	`diamonds` integer DEFAULT 0 NOT NULL,
	`highest_step` integer DEFAULT 0 NOT NULL,
	`revision` integer DEFAULT 0 NOT NULL,
	PRIMARY KEY(`game_id`, `user_id`, `scope`),
	FOREIGN KEY (`user_id`) REFERENCES `game_users`(`id`) ON UPDATE no action ON DELETE cascade
);
--> statement-breakpoint
ALTER TABLE `ranked_matches` ADD `rank_a_before` text;--> statement-breakpoint
ALTER TABLE `ranked_matches` ADD `rank_a_after` text;--> statement-breakpoint
ALTER TABLE `ranked_matches` ADD `rank_b_before` text;--> statement-breakpoint
ALTER TABLE `ranked_matches` ADD `rank_b_after` text;--> statement-breakpoint
ALTER TABLE `ranked_matches` ADD `reward_a` integer DEFAULT 0 NOT NULL;--> statement-breakpoint
ALTER TABLE `ranked_matches` ADD `reward_b` integer DEFAULT 0 NOT NULL;--> statement-breakpoint
ALTER TABLE `ranked_profiles` ADD `progression_version` integer DEFAULT 1 NOT NULL;--> statement-breakpoint
ALTER TABLE `ranked_profiles` ADD `ladder_step` integer DEFAULT 0 NOT NULL;--> statement-breakpoint
ALTER TABLE `ranked_profiles` ADD `level_wins` integer DEFAULT 0 NOT NULL;--> statement-breakpoint
ALTER TABLE `ranked_profiles` ADD `level_losses` integer DEFAULT 0 NOT NULL;--> statement-breakpoint
ALTER TABLE `ranked_profiles` ADD `last_ranked_at` integer;--> statement-breakpoint
ALTER TABLE `ranked_profiles` ADD `decay_applied` integer DEFAULT 0 NOT NULL;--> statement-breakpoint
ALTER TABLE `ranked_profiles` ADD `revision` integer DEFAULT 0 NOT NULL;