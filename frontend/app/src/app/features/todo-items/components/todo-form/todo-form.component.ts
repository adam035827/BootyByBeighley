import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  OnInit,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TodoItemsClient } from '../../../../core/api/api';
import { ErrorService } from '../../../../core/services/error.service';

@Component({
  selector: 'app-todo-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './todo-form.component.html',
  styleUrl: './todo-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TodoFormComponent implements OnInit {
  /** Bound from the :id route parameter via withComponentInputBinding(). Undefined in create mode. */
  readonly id = input<string | undefined>(undefined);

  private readonly fb = inject(FormBuilder);
  private readonly client = inject(TodoItemsClient);
  private readonly errorService = inject(ErrorService);
  private readonly router = inject(Router);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly isEditMode = computed(() => !!this.id());

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    isCompleted: [false],
  });

  ngOnInit(): void {
    const id = this.id();
    if (!id) return;

    this.client.getTodoItem(id).subscribe({
      next: item => {
        this.form.patchValue({
          title: item.title,
          description: item.description ?? '',
          isCompleted: item.isCompleted,
        });
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(this.errorService.extractMessage(err));
      },
    });
  }

  submit(): void {
    if (this.form.invalid) return;

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const { title, description, isCompleted } = this.form.getRawValue();
    const descriptionValue = description.trim() || null;
    const id = this.id();

    const request$ = id
      ? this.client.updateTodoItem(id, { title, description: descriptionValue, isCompleted })
      : this.client.createTodoItem({ title, description: descriptionValue });

    request$.subscribe({
      next: () => this.router.navigate(['/todo-items']),
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(this.errorService.extractMessage(err));
        this.isSubmitting.set(false);
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/todo-items']);
  }
}

